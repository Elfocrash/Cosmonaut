using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Exceptions;
using Cosmonaut.Extensions;
using Cosmonaut.Response;
using Cosmonaut.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public class CosmosStore<TEntity> : ICosmosStore<TEntity> where TEntity : class
    {
        private readonly string _databaseName;
        private int _collectionThrouput = CosmosStoreSettings.DefaultCollectionThroughput;
        private AsyncLazy<Database> _database;
        private AsyncLazy<DocumentCollection> _collection;
        public readonly CosmosStoreSettings Settings;
        private string _collectionName;
        private bool _isUpscaled;
        private readonly IDatabaseCreator _databaseCreator;
        private readonly ICollectionCreator<TEntity> _collectionCreator;

        public CosmosStore(CosmosStoreSettings settings, 
            IDatabaseCreator databaseCreator, 
            ICollectionCreator<TEntity> collectionCreator)
        {
            _collectionCreator = collectionCreator ?? throw new ArgumentNullException(nameof(collectionCreator));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _databaseCreator = databaseCreator ?? throw new ArgumentNullException(nameof(databaseCreator));
            var endpointUrl = Settings.EndpointUrl ?? throw new ArgumentNullException(nameof(Settings.EndpointUrl));
            var authKey = Settings.AuthKey ?? throw new ArgumentNullException(nameof(Settings.AuthKey));
            DocumentClient = DocumentClientFactory.CreateDocumentClient(endpointUrl, authKey);
            _databaseName = Settings.DatabaseName ?? throw new ArgumentNullException(nameof(Settings.DatabaseName));
            InitialiseCosmosStore();
        }
        
        internal CosmosStore(IDocumentClient documentClient, 
            string databaseName, 
            IDatabaseCreator databaseCreator, 
            ICollectionCreator<TEntity> collectionCreator)
        {
            _databaseCreator = databaseCreator ?? throw new ArgumentNullException(nameof(databaseCreator));
            _collectionCreator = collectionCreator ?? throw new ArgumentNullException(nameof(collectionCreator));
            DocumentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(documentClient));
            Settings = new CosmosStoreSettings(databaseName, documentClient.ServiceEndpoint, documentClient.AuthKey.ToString(), documentClient.ConnectionPolicy);
            InitialiseCosmosStore();
        }

        internal CosmosStore(IDocumentClient documentClient,
            string databaseName) : this(documentClient,databaseName,new CosmosDatabaseCreator(documentClient), new CosmosCollectionCreator<TEntity>(documentClient))
        {
        }

        public async Task<CosmosResponse<TEntity>> AddAsync(TEntity entity)
        {
            var collection = await _collection;
            var safeDocument = entity.GetCosmosDbFriendlyEntity();

            try
            {
                ResourceResponse<Document> addedDocument =
                    await DocumentClient.CreateDocumentAsync(collection.SelfLink, safeDocument, new RequestOptions
                    {
                        PartitionKey = entity.GetPartitionKeyValueForEntity()
                    });
                return new CosmosResponse<TEntity>(entity, addedDocument);
            }
            catch (DocumentClientException exception)
            {
                return HandleDocumentClientException(entity, exception);
            }
        }

        public async Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(params TEntity[] entities)
        {
            return await AddRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return new CosmosMultipleResponse<TEntity>();
            var collection = await _collection;
            try
            {
                
                await HandlePotentialUpscalingForRangeOperation(entitiesList, collection, AddAsync);
                var addEntitiesTasks = entitiesList.Select(AddAsync);
                var operationResult = await HandleOperationWithRateLimitRetry(addEntitiesTasks, AddAsync);
                await DownscaleCollectionRequestUnitsToDefault(collection);
                return operationResult;
            }
            catch (Exception)
            {
                await DownscaleCollectionRequestUnitsToDefault(collection);
                return new CosmosMultipleResponse<TEntity>(CosmosOperationStatus.GeneralFailure);
            }
        }

        public async Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return (await QueryableAsync())
                .Where(predicate);
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> RemoveAsync(Func<TEntity, bool> predicate)
        {
            var entitiesToRemove = await ToListAsync(predicate);
            return await RemoveRangeAsync(entitiesToRemove);
        }

        public async Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity)
        {
            try
            {
                entity.ValidateEntityForCosmosDb();
                var documentId = entity.GetDocumentId();
                var documentSelfLink = DocumentHelpers.GetDocumentSelfLink(_databaseName, _collectionName, documentId);
                var result = await DocumentClient.DeleteDocumentAsync(documentSelfLink, new RequestOptions
                {
                    PartitionKey = entity.GetPartitionKeyValueForEntity()
                });
                return new CosmosResponse<TEntity>(entity, result);
            }
            catch (DocumentClientException exception)
            {
                return HandleDocumentClientException(entity, exception);
            }
        }

        public async Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(params TEntity[] entities)
        {
            return await RemoveRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            var entitiesList = entities.ToList();
            if(!entitiesList.Any())
                return new CosmosMultipleResponse<TEntity>();

            var collection = await _collection;
            try
            {
                await HandlePotentialUpscalingForRangeOperation(entitiesList, collection, RemoveAsync);
                var removeEntitiesTasks = entitiesList.Select(RemoveAsync);
                var operationResult = await HandleOperationWithRateLimitRetry(removeEntitiesTasks, RemoveAsync);
                await DownscaleCollectionRequestUnitsToDefault(collection);
                return operationResult;
            }
            catch (Exception)
            {
                //TODO Handle exception
                await DownscaleCollectionRequestUnitsToDefault(collection);
                return new CosmosMultipleResponse<TEntity>(CosmosOperationStatus.GeneralFailure);
            }
            
        }

        public async Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity)
        {
            try
            {
                entity.ValidateEntityForCosmosDb();
                var documentId = entity.GetDocumentId();
                var collection = (await _collection);
                var documentExists = DocumentClient.CreateDocumentQuery<Document>(collection.DocumentsLink, new FeedOptions
                    {
                        EnableCrossPartitionQuery = typeof(TEntity).HasPartitionKey()
                })
                    .Where(x => x.Id == documentId).ToList().SingleOrDefault();

                if (documentExists == null)
                    return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceNotFound);

                var document = entity.GetCosmosDbFriendlyEntity();
                var result = await DocumentClient.UpsertDocumentAsync(collection.DocumentsLink, document, new RequestOptions
                {
                    PartitionKey = entity.GetPartitionKeyValueForEntity()
                });
                return new CosmosResponse<TEntity>(entity, result);
            }
            catch (DocumentClientException exception)
            {
                return HandleDocumentClientException(exception);
            }
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(params TEntity[] entities)
        {
            return await UpdateRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            var entitiesList = entities.ToList();

            if(!entitiesList.Any())
                return new CosmosMultipleResponse<TEntity>();

            var collection = await _collection;
            try
            { 
                await HandlePotentialUpscalingForRangeOperation(entitiesList, collection, UpdateAsync);
                var updateEntitiesTasks = entitiesList.Select(UpdateAsync);
                var operationResult = await HandleOperationWithRateLimitRetry(updateEntitiesTasks, UpdateAsync);
                await DownscaleCollectionRequestUnitsToDefault(collection);
                return operationResult;
            }
            catch (Exception)
            {
                await DownscaleCollectionRequestUnitsToDefault(collection);
                return new CosmosMultipleResponse<TEntity>(CosmosOperationStatus.GeneralFailure);
            }
        }

        public async Task<CosmosResponse<TEntity>> UpsertAsync(TEntity entity)
        {
            try
            {
                entity.ValidateEntityForCosmosDb();
                var collection = (await _collection);
                var document = entity.GetCosmosDbFriendlyEntity();
                ResourceResponse<Document> result = await DocumentClient.UpsertDocumentAsync(collection.DocumentsLink, document, new RequestOptions
                {
                    PartitionKey = entity.GetPartitionKeyValueForEntity()
                });
                return new CosmosResponse<TEntity>(entity, result);
            }
            catch (DocumentClientException exception)
            {
                return HandleDocumentClientException(entity, exception);
            }
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(IEnumerable<TEntity> entities)
        {
            var entitiesList = entities.ToList();
            if(!entitiesList.Any())
                return new CosmosMultipleResponse<TEntity>();

            var collection = await _collection;
            try
            {
                await HandlePotentialUpscalingForRangeOperation(entitiesList, collection, UpdateAsync);
                var upsertEntitiesTasks = entitiesList.Select(UpsertAsync);
                var operationResult = await HandleOperationWithRateLimitRetry(upsertEntitiesTasks, UpsertAsync);
                await DownscaleCollectionRequestUnitsToDefault(collection);
                return operationResult;
            }
            catch (Exception)
            {
                await DownscaleCollectionRequestUnitsToDefault(collection);
                return new CosmosMultipleResponse<TEntity>(CosmosOperationStatus.GeneralFailure);
            }
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(params TEntity[] entities)
        {
            return await UpsertRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id)
        {
            var documentSelfLink = DocumentHelpers.GetDocumentSelfLink(_databaseName, _collectionName, id);
            try
            {
                var result = await DocumentClient.DeleteDocumentAsync(documentSelfLink);
                return new CosmosResponse<TEntity>(result);
            }
            catch (DocumentClientException exception)
            {
                return HandleDocumentClientException(exception);
            }
        }

        public async Task<List<TEntity>> ToListAsync(Func<TEntity, bool> predicate = null)
        {
            if (predicate == null)
            {
                predicate = entity => true;
            }

            return DocumentClient.CreateDocumentQuery<TEntity>((await _collection).DocumentsLink, new FeedOptions
                {
                    EnableCrossPartitionQuery = typeof(TEntity).HasPartitionKey()
                })
                .Where(predicate)
                .ToList();
        }

        public async Task<TEntity> FirstOrDefaultAsync(Func<TEntity, bool> predicate)
        {
            return
                (await QueryableAsync())
                    .FirstOrDefault(predicate);
        }
        
        public IDocumentClient DocumentClient { get; }

        public async Task<IOrderedQueryable<TEntity>> QueryableAsync()
        {
            return DocumentClient.CreateDocumentQuery<TEntity>((await _collection).DocumentsLink, new FeedOptions
            {
                EnableCrossPartitionQuery = typeof(TEntity).HasPartitionKey()
            });
        }

        internal async Task<CosmosMultipleResponse<TEntity>> HandleOperationWithRateLimitRetry(IEnumerable<Task<CosmosResponse<TEntity>>> entitiesTasks,
            Func<TEntity, Task<CosmosResponse<TEntity>>> operationFunc)
        {
            var response = new CosmosMultipleResponse<TEntity>();
            var results = (await Task.WhenAll(entitiesTasks)).ToList();

            async Task RetryPotentialRateLimitFailures()
            {
                var failedBecauseOfRateLimit =
                    results.Where(x => x.CosmosOperationStatus == CosmosOperationStatus.RequestRateIsLarge).ToList();
                if (!failedBecauseOfRateLimit.Any())
                    return;

                results.RemoveAll(x => x.CosmosOperationStatus == CosmosOperationStatus.RequestRateIsLarge);
                entitiesTasks = failedBecauseOfRateLimit.Select(entity => operationFunc(entity.Entity));
                results.AddRange(await Task.WhenAll(entitiesTasks));
                await RetryPotentialRateLimitFailures();
            }

            await RetryPotentialRateLimitFailures();
            response.FailedEntities.AddRange(results.Where(x => !x.IsSuccess));
            return response;
        }
        
        private async Task HandlePotentialUpscalingForRangeOperation(List<TEntity> entitiesList, DocumentCollection collection, Func<TEntity, Task<CosmosResponse<TEntity>>> operationAsync)
        {
            if (Settings.ScaleCollectionRUsAutomatically)
            {
                var sampleEntity = entitiesList.First();
                var sampleResponse = await operationAsync(sampleEntity);
                if (sampleResponse.IsSuccess)
                {
                    entitiesList.Remove(sampleEntity);
                    var requestCharge = sampleResponse.ResourceResponse.RequestCharge;
                    await UpscaleCollectionRequestUnitsForRequest(collection, entitiesList.Count, requestCharge);
                }
            }
        }

        internal async Task<Database> GetDatabaseAsync()
        {
            await _databaseCreator.EnsureCreatedAsync(_databaseName);

            Database database = DocumentClient.CreateDatabaseQuery()
                .Where(db => db.Id == _databaseName)
                .ToArray()
                .FirstOrDefault();
            
            return database;
        }

        internal async Task<DocumentCollection> GetCollectionAsync()
        {
            var database = await _database;
            await _collectionCreator.EnsureCreatedAsync(typeof(TEntity), database, _collectionThrouput);

            var collection = DocumentClient
                .CreateDocumentCollectionQuery(database.SelfLink)
                .ToArray()
                .FirstOrDefault(c => c.Id == _collectionName);

            await AdjustCollectionThroughput(collection);

            return collection;
        }

        internal async Task AdjustCollectionThroughput(DocumentCollection collection)
        {
            var collectionOffer = (OfferV2) DocumentClient.CreateOfferQuery()
                .Where(x => x.ResourceLink == collection.SelfLink).AsEnumerable().Single();
            var currentOfferThroughput = collectionOffer.Content.OfferThroughput;

            if (Settings.AdjustCollectionThroughputOnStartup)
            {
                if (_collectionThrouput != currentOfferThroughput)
                {
                    var updated =
                        await DocumentClient.ReplaceOfferAsync(new OfferV2(collectionOffer, _collectionThrouput));
                    if (updated.StatusCode != HttpStatusCode.OK)
                        throw new CosmosCollectionThroughputUpdateException(collection);
                }
            }
            _collectionThrouput = currentOfferThroughput;
        }

        internal void PingCosmosInOrderToOpenTheClientAndPreventInitialDelay()
        {
            DocumentClient.ReadDatabaseAsync(_database.GetAwaiter().GetResult().SelfLink).Wait();
            DocumentClient.ReadDocumentCollectionAsync(_collection.GetAwaiter().GetResult().SelfLink).Wait();
        }

        internal async Task UpscaleCollectionRequestUnitsForRequest(DocumentCollection collection, int documentCount, double operationCost)
        {
            if (!Settings.ScaleCollectionRUsAutomatically)
                return;

            if (_collectionThrouput >= documentCount * operationCost)
                return;

            var upscaleRequestUnits = (int)(Math.Round(documentCount * operationCost / 100d, 0) * 100);

            var collectionOffer = (OfferV2)DocumentClient.CreateOfferQuery()
                .Where(x => x.ResourceLink == collection.SelfLink).AsEnumerable().Single();
            _collectionThrouput = upscaleRequestUnits >= Settings.MaximumUpscaleRequestUnits ? Settings.MaximumUpscaleRequestUnits : upscaleRequestUnits;
            var replaced = await DocumentClient.ReplaceOfferAsync(new OfferV2(collectionOffer, _collectionThrouput));
            _isUpscaled = replaced.StatusCode == HttpStatusCode.OK;
        }

        internal async Task DownscaleCollectionRequestUnitsToDefault(DocumentCollection collection)
        {
            if (!Settings.ScaleCollectionRUsAutomatically)
                return;

            if (!_isUpscaled)
                return;

            var collectionOffer = (OfferV2)DocumentClient.CreateOfferQuery()
                .Where(x => x.ResourceLink == collection.SelfLink).AsEnumerable().Single();
            _collectionThrouput = typeof(TEntity).GetCollectionThroughputForEntity(Settings.AllowAttributesToConfigureThroughput, Settings.CollectionThroughput);
            var replaced = await DocumentClient.ReplaceOfferAsync(new OfferV2(collectionOffer, _collectionThrouput));
            _isUpscaled = replaced.StatusCode != HttpStatusCode.OK;
        }

        internal void InitialiseCosmosStore()
        {
            _collectionName = typeof(TEntity).GetCollectionName();
            _collectionThrouput = typeof(TEntity).GetCollectionThroughputForEntity(Settings.AllowAttributesToConfigureThroughput, Settings.CollectionThroughput);

            _database = new AsyncLazy<Database>(async () => await GetDatabaseAsync());
            _collection = new AsyncLazy<DocumentCollection>(async () => await GetCollectionAsync());

            PingCosmosInOrderToOpenTheClientAndPreventInitialDelay();
        }

        internal CosmosResponse<TEntity> HandleDocumentClientException(TEntity entity, DocumentClientException exception)
        {
            if (exception.Message.Contains("Resource Not Found"))
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceNotFound);

            if (exception.Message.Contains("Request rate is large"))
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.RequestRateIsLarge);

            if (exception.Message.Contains("Resource with specified id or name already exists"))
                return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceWithIdAlreadyExists);

            throw exception;
        }

        internal CosmosResponse<TEntity> HandleDocumentClientException(DocumentClientException exception)
        {
            return HandleDocumentClientException(null, exception);
        }
    }
}