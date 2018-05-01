using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Operations;
using Cosmonaut.Response;
using Cosmonaut.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Cosmonaut
{
    public sealed class CosmosStore<TEntity> : ICosmosStore<TEntity> where TEntity : class
    {
        public IDocumentClient DocumentClient { get; }

        public int CollectionThrouput { get; internal set; } = CosmosConstants.MinimumCosmosThroughput;

        public bool IsUpscaled { get; internal set; }

        public bool IsShared { get; internal set; }

        public string CollectionName { get; private set; }

        public CosmosStoreSettings Settings { get; }

        private AsyncLazy<Database> _database;
        private AsyncLazy<DocumentCollection> _collection;
        private readonly IDatabaseCreator _databaseCreator;
        private readonly ICollectionCreator _collectionCreator;
        private readonly CosmosScaler<TEntity> _cosmosScaler;

        public CosmosStore(CosmosStoreSettings settings,
            IDatabaseCreator databaseCreator,
            ICollectionCreator collectionCreator)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            var endpointUrl = Settings.EndpointUrl ?? throw new ArgumentNullException(nameof(Settings.EndpointUrl));
            var authKey = Settings.AuthKey ?? throw new ArgumentNullException(nameof(Settings.AuthKey));
            DocumentClient = DocumentClientFactory.CreateDocumentClient(endpointUrl, authKey);
            if (string.IsNullOrEmpty(Settings.DatabaseName)) throw new ArgumentNullException(nameof(Settings.DatabaseName));
            _collectionCreator = collectionCreator ?? throw new ArgumentNullException(nameof(collectionCreator));
            _databaseCreator = databaseCreator ?? throw new ArgumentNullException(nameof(databaseCreator));
            _cosmosScaler = new CosmosScaler<TEntity>(this);
            InitialiseCosmosStore();
        }

        internal CosmosStore(IDocumentClient documentClient,
            string databaseName,
            IDatabaseCreator databaseCreator,
            ICollectionCreator collectionCreator,
            bool scaleable = false,
            bool allowAttributesToSetThrouput = false,
            bool adjustThroughputOnStartup = false)
        {
            DocumentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
            Settings = new CosmosStoreSettings(databaseName, documentClient.ServiceEndpoint, documentClient.AuthKey.ToString(), documentClient.ConnectionPolicy, 
                scaleCollectionRUsAutomatically: scaleable, 
                allowAttributesToConfigureThroughput: allowAttributesToSetThrouput,
                adjustCollectionThroughputOnStartup: adjustThroughputOnStartup);
            if (string.IsNullOrEmpty(Settings.DatabaseName)) throw new ArgumentNullException(nameof(Settings.DatabaseName));
            _databaseCreator = databaseCreator ?? throw new ArgumentNullException(nameof(databaseCreator));
            _collectionCreator = collectionCreator ?? throw new ArgumentNullException(nameof(collectionCreator));
            _cosmosScaler = new CosmosScaler<TEntity>(this);
            InitialiseCosmosStore();
        }

        internal CosmosStore(IDocumentClient documentClient,
            string databaseName) : this(documentClient, databaseName,
            new CosmosDatabaseCreator(documentClient),
            new CosmosCollectionCreator(documentClient))
        {
        }

        public async Task<CosmosResponse<TEntity>> AddAsync(TEntity entity)
        {
            var collection = await _collection;
            var safeDocument = entity.GetCosmosDbFriendlyEntity();
            
            try
            {
                ResourceResponse<Document> addedDocument =
                    await DocumentClient.CreateDocumentAsync(collection.SelfLink, safeDocument, GetRequestOptions(entity));
                return new CosmosResponse<TEntity>(entity, addedDocument);
            }
            catch (Exception exception)
            {
                return exception.HandleOperationException(entity);
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
                var multipleResponse = await _cosmosScaler.UpscaleCollectionIfConfiguredAsSuch(entitiesList, collection, AddAsync);
                var addEntitiesTasks = entitiesList.Select(AddAsync);
                var operationResult = await HandleOperationWithRateLimitRetry(addEntitiesTasks, AddAsync);
                multipleResponse.SuccessfulEntities.AddRange(operationResult.SuccessfulEntities);
                multipleResponse.FailedEntities.AddRange(operationResult.FailedEntities);
                await _cosmosScaler.DownscaleCollectionRequestUnitsToDefault(collection);
                return multipleResponse;
            }
            catch (Exception exception)
            {
                await _cosmosScaler.DownscaleCollectionRequestUnitsToDefault(collection);
                return new CosmosMultipleResponse<TEntity>(exception);
            }
        }

        public async Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (IsShared)
                ExpressionExtensions.AddSharedCollectionFilter(ref predicate);

            return DocumentClient.CreateDocumentQuery<TEntity>((await _collection).DocumentsLink, new FeedOptions
            {
                EnableCrossPartitionQuery = typeof(TEntity).HasPartitionKey() || IsShared
            })
                .Where(predicate);
        }

        public async Task<IDocumentQuery<TEntity>> AsDocumentQueryAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                predicate = entity => true;
            }

            return (await WhereAsync(predicate)).AsDocumentQuery();
        }

        public async Task<CosmosMultipleResponse<TEntity>> RemoveAsync(Expression<Func<TEntity, bool>> predicate)
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
                var documentUri = UriFactory.CreateDocumentUri(Settings.DatabaseName, CollectionName, documentId);
                var result = await DocumentClient.DeleteDocumentAsync(documentUri, GetRequestOptions(entity));
                return new CosmosResponse<TEntity>(entity, result);
            }
            catch (Exception exception)
            {
                return exception.HandleOperationException(entity);
            }
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(params TEntity[] entities)
        {
            return await RemoveRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(IEnumerable<TEntity> entities)
        {
            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return new CosmosMultipleResponse<TEntity>();

            var collection = await _collection;
            try
            {
                var multipleResponse = await _cosmosScaler.UpscaleCollectionIfConfiguredAsSuch(entitiesList, collection, RemoveAsync);
                var removeEntitiesTasks = entitiesList.Select(RemoveAsync);
                var operationResult = await HandleOperationWithRateLimitRetry(removeEntitiesTasks, RemoveAsync);
                multipleResponse.SuccessfulEntities.AddRange(operationResult.SuccessfulEntities);
                multipleResponse.FailedEntities.AddRange(operationResult.FailedEntities);
                await _cosmosScaler.DownscaleCollectionRequestUnitsToDefault(collection);
                return multipleResponse;
            }
            catch (Exception exception)
            {
                await _cosmosScaler.DownscaleCollectionRequestUnitsToDefault(collection);
                return new CosmosMultipleResponse<TEntity>(exception);
            }
        }

        public async Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity)
        {
            try
            {
                entity.ValidateEntityForCosmosDb();
                var documentId = entity.GetDocumentId();
                var document = entity.GetCosmosDbFriendlyEntity();
                var documentUri = UriFactory.CreateDocumentUri(Settings.DatabaseName, CollectionName, documentId);
                var result = await DocumentClient.ReplaceDocumentAsync(documentUri, document, GetRequestOptions(entity));
                return new CosmosResponse<TEntity>(entity, result);
            }
            catch (Exception exception)
            {
                return exception.HandleOperationException(entity);
            }
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(params TEntity[] entities)
        {
            return await UpdateRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            var entitiesList = entities.ToList();

            if (!entitiesList.Any())
                return new CosmosMultipleResponse<TEntity>();

            var collection = await _collection;
            try
            {
                var multipleResponse = await _cosmosScaler.UpscaleCollectionIfConfiguredAsSuch(entitiesList, collection, UpdateAsync);
                var updateEntitiesTasks = entitiesList.Select(UpdateAsync);
                var operationResult = await HandleOperationWithRateLimitRetry(updateEntitiesTasks, UpdateAsync);
                multipleResponse.SuccessfulEntities.AddRange(operationResult.SuccessfulEntities);
                multipleResponse.FailedEntities.AddRange(operationResult.FailedEntities);
                await _cosmosScaler.DownscaleCollectionRequestUnitsToDefault(collection);
                return multipleResponse;
            }
            catch (Exception exception)
            {
                await _cosmosScaler.DownscaleCollectionRequestUnitsToDefault(collection);
                return new CosmosMultipleResponse<TEntity>(exception);
            }
        }

        public async Task<CosmosResponse<TEntity>> UpsertAsync(TEntity entity)
        {
            try
            {
                entity.ValidateEntityForCosmosDb();
                var collection = (await _collection);
                var document = entity.GetCosmosDbFriendlyEntity();
                ResourceResponse<Document> result = await DocumentClient.UpsertDocumentAsync(collection.DocumentsLink, document, GetRequestOptions(entity));
                return new CosmosResponse<TEntity>(entity, result);
            }
            catch (Exception exception)
            {
                return exception.HandleOperationException(entity);
            }
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(IEnumerable<TEntity> entities)
        {
            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return new CosmosMultipleResponse<TEntity>();

            var collection = await _collection;
            try
            {
                var multipleResponse = await _cosmosScaler.UpscaleCollectionIfConfiguredAsSuch(entitiesList, collection, UpsertAsync);
                var upsertEntitiesTasks = entitiesList.Select(UpsertAsync);
                var operationResult = await HandleOperationWithRateLimitRetry(upsertEntitiesTasks, UpsertAsync);
                multipleResponse.SuccessfulEntities.AddRange(operationResult.SuccessfulEntities);
                multipleResponse.FailedEntities.AddRange(operationResult.FailedEntities);
                await _cosmosScaler.DownscaleCollectionRequestUnitsToDefault(collection);
                return multipleResponse;
            }
            catch (Exception exception)
            {
                await _cosmosScaler.DownscaleCollectionRequestUnitsToDefault(collection);
                return new CosmosMultipleResponse<TEntity>(exception);
            }
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(params TEntity[] entities)
        {
            return await UpsertRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id)
        {
            try
            {
                var documentUri = UriFactory.CreateDocumentUri(Settings.DatabaseName, CollectionName, id);
                var result = await DocumentClient.DeleteDocumentAsync(documentUri);
                return new CosmosResponse<TEntity>(result);
            }
            catch (Exception exception)
            {
                return exception.HandleOperationException<TEntity>();
            }
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                predicate = entity => true;
            }

            if (IsShared)
                ExpressionExtensions.AddSharedCollectionFilter(ref predicate);

            var queryable = DocumentClient.CreateDocumentQuery<TEntity>((await _collection).DocumentsLink,
                new FeedOptions
                {
                    EnableCrossPartitionQuery = typeof(TEntity).HasPartitionKey() || IsShared
                });
            var filter = queryable.Where(predicate);
            var count = await filter.CountAsync(cancellationToken);

            return count;
        }

        public async Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
            {
                predicate = entity => true;
            }

            if (IsShared)
                ExpressionExtensions.AddSharedCollectionFilter(ref predicate);

            var queryable = DocumentClient.CreateDocumentQuery<TEntity>((await _collection).DocumentsLink, new FeedOptions
            {
                EnableCrossPartitionQuery = typeof(TEntity).HasPartitionKey() || IsShared
            });
            var filter = queryable.Where(predicate);
            var query = filter.AsDocumentQuery();

            var result = new List<TEntity>();
            while (query.HasMoreResults)
            {
                var item = await query.ExecuteNextAsync<TEntity>(cancellationToken);
                result.AddRange(item);
            }
            return result;
        }
        
        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (IsShared)
                ExpressionExtensions.AddSharedCollectionFilter(ref predicate);

            var query = DocumentClient.CreateDocumentQuery<TEntity>((await _collection).DocumentsLink, new FeedOptions
            {
                EnableCrossPartitionQuery = typeof(TEntity).HasPartitionKey() || IsShared
            })
            .Where(predicate)
            .AsDocumentQuery();

            if (!query.HasMoreResults) return null;
            var item = await query.ExecuteNextAsync<TEntity>(cancellationToken);
            return item.FirstOrDefault();
        }

        private async Task<CosmosMultipleResponse<TEntity>> HandleOperationWithRateLimitRetry(IEnumerable<Task<CosmosResponse<TEntity>>> entitiesTasks,
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
            response.SuccessfulEntities.AddRange(results.Where(x => x.IsSuccess));
            return response;
        }

        private async Task<Database> GetDatabaseAsync()
        {
            await _databaseCreator.EnsureCreatedAsync(Settings.DatabaseName);

            Database database = DocumentClient.CreateDatabaseQuery()
                .Where(db => db.Id == Settings.DatabaseName)
                .ToArray()
                .First();

            return database;
        }

        private async Task<DocumentCollection> GetCollectionAsync()
        {
            var database = await _database;
            await _collectionCreator.EnsureCreatedAsync<TEntity>(database, CollectionThrouput, Settings.IndexingPolicy);

            var collection = DocumentClient
                .CreateDocumentCollectionQuery(database.SelfLink)
                .ToArray()
                .FirstOrDefault(c => c.Id == CollectionName);

            await _cosmosScaler.AdjustCollectionThroughput(collection);

            return collection;
        }

        private void PingCosmosInOrderToOpenTheClientAndPreventInitialDelay()
        {
            DocumentClient.ReadDatabaseAsync(_database.GetAwaiter().GetResult().SelfLink).GetAwaiter().GetResult();
            DocumentClient.ReadDocumentCollectionAsync(_collection.GetAwaiter().GetResult().SelfLink).GetAwaiter().GetResult();
        }
        
        private void InitialiseCosmosStore()
        {
            IsShared = typeof(TEntity).UsesSharedCollection();
            CollectionName = IsShared ? typeof(TEntity).GetSharedCollectionName() : typeof(TEntity).GetCollectionName();
            CollectionThrouput = typeof(TEntity).GetCollectionThroughputForEntity(Settings.AllowAttributesToConfigureThroughput, Settings.CollectionThroughput);

            _database = new AsyncLazy<Database>(async () => await GetDatabaseAsync());
            _collection = new AsyncLazy<DocumentCollection>(async () => await GetCollectionAsync());

            PingCosmosInOrderToOpenTheClientAndPreventInitialDelay();
        }
        
        private RequestOptions GetRequestOptions(TEntity entity)
        {
            var partitionKeyValue = entity.GetPartitionKeyValueForEntity(IsShared);
            return partitionKeyValue != null ? new RequestOptions
            {
                PartitionKey = entity.GetPartitionKeyValueForEntity(IsShared)
            } : null;
        }
    }
}