using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Diagnostics;
using Cosmonaut.Extensions;
using Cosmonaut.Operations;
using Cosmonaut.Response;
using Cosmonaut.Storage;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

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

        public string DatabaseName { get; private set; }

        private readonly IDatabaseCreator _databaseCreator;
        private readonly ICollectionCreator _collectionCreator;
        private readonly CosmosScaler<TEntity> _cosmosScaler;

        public CosmosStore(CosmosStoreSettings settings) : this(settings, string.Empty)
        {
        }

        public CosmosStore(IDocumentClient documentClient,
            string databaseName,
            string authKey, 
            string endpoint) : this(documentClient, databaseName, authKey, endpoint, string.Empty,
            new CosmosDatabaseCreator(documentClient),
            new CosmosCollectionCreator(documentClient))
        {
        }

        public CosmosStore(IDocumentClient documentClient,
            string databaseName,
            string authKey,
            string endpoint,
            string overriddenCollectionName) : this(documentClient, 
            databaseName, 
            authKey,
            endpoint,
            overriddenCollectionName,
            new CosmosDatabaseCreator(documentClient),
            new CosmosCollectionCreator(documentClient))
        {
        }

        public CosmosStore(CosmosStoreSettings settings, string overriddenCollectionName)
        {
            CollectionName = overriddenCollectionName;
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            DatabaseName = settings.DatabaseName;
            DocumentClient = DocumentClientFactory.CreateDocumentClient(settings);
            if (string.IsNullOrEmpty(Settings.DatabaseName)) throw new ArgumentNullException(nameof(Settings.DatabaseName));
            _collectionCreator = new CosmosCollectionCreator(DocumentClient);
            _databaseCreator = new CosmosDatabaseCreator(DocumentClient);
            _cosmosScaler = new CosmosScaler<TEntity>(this);
            InitialiseCosmosStore();
        }

        internal CosmosStore(IDocumentClient documentClient,
            string databaseName,
            string authKey,
            string endpoint,
            string overriddenCollectionName,
            IDatabaseCreator databaseCreator = null,
            ICollectionCreator collectionCreator = null,
            bool scaleable = false)
        {
            CollectionName = overriddenCollectionName;
            DatabaseName = databaseName;
            DocumentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
            Settings = new CosmosStoreSettings(databaseName, endpoint, authKey, documentClient.ConnectionPolicy, 
                scaleCollectionRUsAutomatically: scaleable);
            if (string.IsNullOrEmpty(Settings.DatabaseName)) throw new ArgumentNullException(nameof(Settings.DatabaseName));
            _collectionCreator = collectionCreator ?? new CosmosCollectionCreator(DocumentClient);
            _databaseCreator = databaseCreator ?? new CosmosDatabaseCreator(DocumentClient);
            _cosmosScaler = new CosmosScaler<TEntity>(this);
            InitialiseCosmosStore();
        }

        public IQueryable<TEntity> Query(FeedOptions feedOptions = null)
        {
            var queryable = DocumentClient.CreateDocumentQuery<TEntity>(CollectionLink, GetFeedOptionsForQuery(feedOptions));

            return IsShared ? queryable.Where(ExpressionExtensions.SharedCollectionExpression<TEntity>()) : queryable;
        }

        public async Task<TEntity> QuerySingleAsync(string sql, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            var queryable = GetSqlBasedQueryableForType<TEntity>(sql, feedOptions);

            return await queryable.SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<T> QuerySingleAsync<T>(string sql, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            var queryable = GetSqlBasedQueryableForType<T>(sql, feedOptions);

            return await queryable.SingleOrDefaultGenericAsync(cancellationToken);
        }
        
        public async Task<IEnumerable<TEntity>> QueryMultipleAsync(string sql, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            var queryable = GetSqlBasedQueryableForType<TEntity>(sql, feedOptions);

            return await queryable.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> QueryMultipleAsync<T>(string sql, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
        {
            var queryable = GetSqlBasedQueryableForType<T>(sql, feedOptions);

            return await queryable.ToGenericListAsync(cancellationToken);
        }

        public async Task<CosmosResponse<TEntity>> AddAsync(TEntity entity, RequestOptions requestOptions = null)
        {
            var safeDocument = entity.GetCosmosDbFriendlyEntity();
            
            try
            {
                var addedDocument =
                    await this.InvokeCosmosCallAsync<ResourceResponse<Document>>(() => 
                        DocumentClient.CreateDocumentAsync(CollectionLink, safeDocument, GetRequestOptions(requestOptions, entity)), entity.GetDocumentId());
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

        public async Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, RequestOptions requestOptions = null)
        {
            return await ExecuteMultiOperationAsync(entities, x => AddAsync(x, requestOptions));
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> RemoveAsync(
            Expression<Func<TEntity, bool>> predicate, 
            FeedOptions feedOptions = null, 
            RequestOptions requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var entitiesToRemove = await Query(GetFeedOptionsForQuery(feedOptions)).Where(predicate).ToListAsync(cancellationToken);
            return await RemoveRangeAsync(entitiesToRemove, requestOptions);
        }

        public async Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity, RequestOptions requestOptions = null)
        {
            try
            {
                entity.ValidateEntityForCosmosDb();
                var documentId = entity.GetDocumentId();
                var documentUri = UriFactory.CreateDocumentUri(Settings.DatabaseName, CollectionName, documentId);
                var result = await this.InvokeCosmosCallAsync(() => DocumentClient.DeleteDocumentAsync(documentUri, GetRequestOptions(requestOptions, entity)), documentId);
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

        public async Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(IEnumerable<TEntity> entities, RequestOptions requestOptions = null)
        {
            return await ExecuteMultiOperationAsync(entities, x => RemoveAsync(x, requestOptions));
        }

        public async Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity, RequestOptions requestOptions = null)
        {
            try
            {
                entity.ValidateEntityForCosmosDb();
                var documentId = entity.GetDocumentId();
                var document = entity.GetCosmosDbFriendlyEntity();
                var documentUri = UriFactory.CreateDocumentUri(Settings.DatabaseName, CollectionName, documentId);
                var result = await this.InvokeCosmosCallAsync<ResourceResponse<Document>>(() => DocumentClient.ReplaceDocumentAsync(documentUri, document, GetRequestOptions(requestOptions, entity)), documentId);
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

        public async Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, RequestOptions requestOptions = null)
        {
            return await ExecuteMultiOperationAsync(entities, x => UpdateAsync(x, requestOptions));
        }

        public async Task<CosmosResponse<TEntity>> UpsertAsync(TEntity entity, RequestOptions requestOptions = null)
        {
            try
            {
                entity.ValidateEntityForCosmosDb();
                var document = entity.GetCosmosDbFriendlyEntity();
                var result = await this.InvokeCosmosCallAsync<ResourceResponse<Document>>(() => DocumentClient.UpsertDocumentAsync(CollectionLink, document, GetRequestOptions(requestOptions, entity)), entity.GetDocumentId());
                return new CosmosResponse<TEntity>(entity, result);
            }
            catch (Exception exception)
            {
                return exception.HandleOperationException(entity);
            }
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(IEnumerable<TEntity> entities, RequestOptions requestOptions = null)
        {
            return await ExecuteMultiOperationAsync(entities, x => UpsertAsync(x, requestOptions));
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(params TEntity[] entities)
        {
            return await UpsertRangeAsync((IEnumerable<TEntity>)entities);
        }

        public async Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id, RequestOptions requestOptions = null)
        {
            try
            {
                var documentUri = UriFactory.CreateDocumentUri(Settings.DatabaseName, CollectionName, id);
                var result = await this.InvokeCosmosCallAsync(() => DocumentClient.DeleteDocumentAsync(documentUri, GetRequestOptions(id, requestOptions, typeof(TEntity))), id);
                return new CosmosResponse<TEntity>(result);
            }
            catch (Exception exception)
            {
                return exception.HandleOperationException<TEntity>();
            }
        }

        private static async Task<CosmosMultipleResponse<TEntity>> HandleOperationWithRateLimitRetry(IEnumerable<Task<CosmosResponse<TEntity>>> entitiesTasks,
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
        
        private void InitialiseCosmosStore()
        {
            IsShared = typeof(TEntity).UsesSharedCollection();

            if(string.IsNullOrEmpty(CollectionName))
                CollectionName = IsShared ? typeof(TEntity).GetSharedCollectionName() : typeof(TEntity).GetCollectionName();

            CollectionThrouput = typeof(TEntity).GetCollectionThroughputForEntity(Settings.DefaultCollectionThroughput);

            _databaseCreator.EnsureCreatedAsync(DatabaseName).ConfigureAwait(false).GetAwaiter().GetResult();
            _collectionCreator.EnsureCreatedAsync<TEntity>(DatabaseLink.ToString(), CollectionName, CollectionThrouput, Settings.IndexingPolicy);
        }

        private async Task<CosmosMultipleResponse<TEntity>> ExecuteMultiOperationAsync(IEnumerable<TEntity> entities,
            Func<TEntity, Task<CosmosResponse<TEntity>>> operationFunc)
        {
            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return new CosmosMultipleResponse<TEntity>();

            try
            {
                var multipleResponse = await _cosmosScaler.UpscaleCollectionIfConfiguredAsSuch(entitiesList, CollectionLink.ToString(), operationFunc);
                var multiOperationEntitiesTasks = entitiesList.Select(operationFunc);
                var operationResult = await HandleOperationWithRateLimitRetry(multiOperationEntitiesTasks, operationFunc);
                multipleResponse.SuccessfulEntities.AddRange(operationResult.SuccessfulEntities);
                multipleResponse.FailedEntities.AddRange(operationResult.FailedEntities);
                await _cosmosScaler.DownscaleCollectionRequestUnitsToDefault(CollectionLink.ToString());
                return multipleResponse;
            }
            catch (Exception exception)
            {
                await _cosmosScaler.DownscaleCollectionRequestUnitsToDefault(CollectionLink.ToString());
                return new CosmosMultipleResponse<TEntity>(exception);
            }
        }

        private RequestOptions GetRequestOptions(RequestOptions requestOptions, TEntity entity)
        {
            var partitionKeyValue = entity.GetPartitionKeyValueForEntity(IsShared);
            if (requestOptions == null)
            {
                return partitionKeyValue != null ? new RequestOptions
                {
                    PartitionKey = partitionKeyValue
                } : null;
            }

            requestOptions.PartitionKey = partitionKeyValue;
            return requestOptions;
        }

        private RequestOptions GetRequestOptions(string id, RequestOptions requestOptions, Type typeOfEntity)
        {
            var partitionKeyDefinition = typeOfEntity.GetPartitionKeyForEntity();
            var partitionKeyIsId = IsShared || (partitionKeyDefinition?.Paths?.SingleOrDefault()?.Equals($"/{CosmosConstants.CosmosId}") ?? false);
            if (requestOptions == null && partitionKeyIsId)
            {
                return new RequestOptions
                {
                    PartitionKey = new PartitionKey(id)
                };
            }

            if (requestOptions != null && partitionKeyIsId)
                requestOptions.PartitionKey = new PartitionKey(id);

            return requestOptions;
        }

        private FeedOptions GetFeedOptionsForQuery(FeedOptions feedOptions)
        {
            var shouldEnablePartitionQuery = (typeof(TEntity).HasPartitionKey() || IsShared) && feedOptions?.PartitionKey == null;

            if (feedOptions == null)
            {
                return new FeedOptions
                {
                    EnableCrossPartitionQuery = shouldEnablePartitionQuery
                };
            }

            feedOptions.EnableCrossPartitionQuery = shouldEnablePartitionQuery;
            return feedOptions;
        }
        
        private IQueryable<T> GetSqlBasedQueryableForType<T>(string sql, FeedOptions feedOptions)
        {
            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();

            var queryable = DocumentClient.CreateDocumentQuery<T>(CollectionLink, collectionSharingFriendlySql,
                GetFeedOptionsForQuery(feedOptions));
            return queryable;
        }

        private Uri DatabaseLink => UriFactory.CreateDatabaseUri(DatabaseName);

        private Uri CollectionLink => UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
    }
}