using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Cosmonaut.Attributes;
using Cosmonaut.Exceptions;
using Cosmonaut.Extensions;
using Cosmonaut.Response;
using Humanizer;
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
        private CosmosDocumentProcessor<TEntity> _documentProcessor;

        public CosmosStore(CosmosStoreSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            var endpointUrl = Settings.EndpointUrl ?? throw new ArgumentNullException(nameof(Settings.EndpointUrl));
            var authKey = Settings.AuthKey ?? throw new ArgumentNullException(nameof(Settings.AuthKey));
            DocumentClient = CreateDocumentClient(endpointUrl, authKey);
            _databaseName = Settings.DatabaseName ?? throw new ArgumentNullException(nameof(Settings.DatabaseName));
            InitialiseCosmosStore();
        }
        
        internal CosmosStore(IDocumentClient documentClient, string databaseName)
        {   
            DocumentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(documentClient));
            Settings = new CosmosStoreSettings(databaseName, documentClient.ServiceEndpoint, documentClient.AuthKey.ToString(), documentClient.ConnectionPolicy);
            InitialiseCosmosStore();
        }
        
        public async Task<CosmosResponse<TEntity>> AddAsync(TEntity entity)
        {
            var collection = await _collection;
            var safeDocument = _documentProcessor.GetCosmosDbFriendlyEntity(entity);

            try
            {
                ResourceResponse<Document> addedDocument =
                    await DocumentClient.CreateDocumentAsync(collection.SelfLink, safeDocument, new RequestOptions
                    {
                        PartitionKey = _documentProcessor.GetPartitionKeyValueForEntity(entity)
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
            var addEntitiesTasks = entities.Select(AddAsync);
            return await HandleOperationWithRateLimitRetry(addEntitiesTasks, AddAsync);
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
                _documentProcessor.ValidateEntityForCosmosDb(entity);
                var documentId = _documentProcessor.GetDocumentId(entity);
                var documentSelfLink = _documentProcessor.GetDocumentSelfLink(_databaseName,_collectionName, documentId);
                var result = await DocumentClient.DeleteDocumentAsync(documentSelfLink, new RequestOptions
                {
                    PartitionKey = _documentProcessor.GetPartitionKeyValueForEntity(entity)
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
            var removeEntitiesTasks = entities.Select(RemoveAsync);
            return await HandleOperationWithRateLimitRetry(removeEntitiesTasks, RemoveAsync);
        }

        public async Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity)
        {
            try
            {
                _documentProcessor.ValidateEntityForCosmosDb(entity);
                var documentId = _documentProcessor.GetDocumentId(entity);
                var collection = (await _collection);
                var documentExists = DocumentClient.CreateDocumentQuery<Document>(collection.DocumentsLink, new FeedOptions
                    {
                        EnableCrossPartitionQuery = true
                    })
                    .Where(x => x.Id == documentId).ToList().SingleOrDefault();

                if (documentExists == null)
                    return new CosmosResponse<TEntity>(entity, CosmosOperationStatus.ResourceNotFound);

                var document = _documentProcessor.GetCosmosDbFriendlyEntity(entity);
                var result = await DocumentClient.UpsertDocumentAsync(collection.DocumentsLink, document, new RequestOptions
                {
                    PartitionKey = _documentProcessor.GetPartitionKeyValueForEntity(entity)
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
            var updateEntitiesTasks = entities.Select(UpdateAsync);
            return await HandleOperationWithRateLimitRetry(updateEntitiesTasks, UpdateAsync);
        }

        public async Task<CosmosResponse<TEntity>> UpsertAsync(TEntity entity)
        {
            try
            {
                _documentProcessor.ValidateEntityForCosmosDb(entity);
                var collection = (await _collection);
                var document = _documentProcessor.GetCosmosDbFriendlyEntity(entity);
                ResourceResponse<Document> result = await DocumentClient.UpsertDocumentAsync(collection.DocumentsLink, document, new RequestOptions
                {
                    PartitionKey = _documentProcessor.GetPartitionKeyValueForEntity(entity)
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
            var upsertEntitiesTasks = entities.Select(UpsertAsync);
            return await HandleOperationWithRateLimitRetry(upsertEntitiesTasks, UpsertAsync);
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(params TEntity[] entities)
        {
            return await UpsertRangeAsync((IEnumerable<TEntity>)entities);
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

        public async Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id)
        {
            var documentSelfLink = _documentProcessor.GetDocumentSelfLink(_databaseName, _collectionName, id);
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
                    EnableCrossPartitionQuery = true
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
                EnableCrossPartitionQuery = true
            });
        }

        internal async Task<Database> GetOrCreateDatabaseAsync()
        {
            Database database = DocumentClient.CreateDatabaseQuery()
                .Where(db => db.Id == _databaseName)
                .ToArray()
                .FirstOrDefault();

            if (database == null)
            {
                database = await DocumentClient.CreateDatabaseAsync(
                    new Database { Id = _databaseName });
            }

            return database;
        }

        internal async Task<DocumentCollection> GetOrCreateCollectionAsync()
        {
            var collection = DocumentClient
                .CreateDocumentCollectionQuery((await _database).SelfLink)
                .ToArray()
                .FirstOrDefault(c => c.Id == _collectionName);
            
            if (collection == null)
            {
                collection = new DocumentCollection
                {
                    Id = _collectionName
                };
                var partitionKey = _documentProcessor.GetPartitionKeyForEntity(typeof(TEntity));

                if(partitionKey != null)
                    collection.PartitionKey = _documentProcessor.GetPartitionKeyForEntity(typeof(TEntity));

                collection = await DocumentClient.CreateDocumentCollectionAsync((await _database).SelfLink, collection, new RequestOptions
                {
                    OfferThroughput = _collectionThrouput
                });

                return collection;
            }

            var collectionOffer = (OfferV2)DocumentClient.CreateOfferQuery()
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

            return collection;
        }
        
        internal void PingCosmosInOrderToOpenTheClientAndPreventInitialDelay()
        {
            DocumentClient.ReadDatabaseAsync(_database.GetAwaiter().GetResult().SelfLink).Wait();
            DocumentClient.ReadDocumentCollectionAsync(_collection.GetAwaiter().GetResult().SelfLink).Wait();
        }

        internal string GetCollectionNameForEntity()
        {
            var collectionNameAttribute = typeof(TEntity).GetCustomAttribute<CosmosCollectionAttribute>();
            
            var collectionName = collectionNameAttribute?.Name;

            return !string.IsNullOrEmpty(collectionName) ? collectionName : typeof(TEntity).Name.ToLower().Pluralize();
        }

        internal int GetCollectionThroughputForEntity()
        {
            if (!Settings.AllowAttributesToConfigureThroughput)
            {
                EnsureThroughputIsInAcceptableRange(Settings.CollectionThroughput);
                return Settings.CollectionThroughput;
            }

            var collectionNameAttribute = typeof(TEntity).GetCustomAttribute<CosmosCollectionAttribute>();
            var throughput = collectionNameAttribute != null && collectionNameAttribute.Throughput != -1 ? collectionNameAttribute.Throughput : Settings.CollectionThroughput;
            EnsureThroughputIsInAcceptableRange(throughput);
            return throughput;
        }

        internal void EnsureThroughputIsInAcceptableRange(int throughtput)
        {
            if (throughtput < 400)
                throw new IllegalCosmosThroughputException();
        }

        internal void InitialiseCosmosStore()
        {
            _documentProcessor = new CosmosDocumentProcessor<TEntity>();
            _collectionName = GetCollectionNameForEntity();
            _collectionThrouput = GetCollectionThroughputForEntity();

            _database = new AsyncLazy<Database>(async () => await GetOrCreateDatabaseAsync());
            _collection = new AsyncLazy<DocumentCollection>(async () => await GetOrCreateCollectionAsync());

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
        
        internal DocumentClient CreateDocumentClient(Uri endpointUrl, string authKey)
        {
            return new DocumentClient(endpointUrl, authKey, Settings.ConnectionPolicy ?? ConnectionPolicy.Default);
        }
    }
}