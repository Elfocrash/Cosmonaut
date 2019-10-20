
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Factories;
using Cosmonaut.Response;
using Cosmonaut.Storage;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut
{
    public sealed class CosmosStore<TEntity> : ICosmosStore<TEntity> where TEntity : class
    {
        public bool IsShared { get; internal set; }

        public Container Container { get; private set; }
        
        public Database Database { get; }

        public CosmosStoreSettings Settings { get; }
        
        public ICosmonautClient CosmonautClient { get; }

        private readonly IDatabaseCreator _databaseCreator;
        private readonly IContainerCreator _containerCreator;

        public CosmosStore(CosmosStoreSettings settings) : this(settings, string.Empty)
        {
        }

        public CosmosStore(CosmosStoreSettings settings, string overriddenCollectionName)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            var documentClient = CosmosClientFactory.CreateDocumentClient(settings);
            CosmonautClient = new CosmonautClient(documentClient, Settings.InfiniteRetries);
            Database = CosmonautClient.CosmosClient.GetDatabase(settings.DatabaseId);
            
            if (string.IsNullOrEmpty(Settings.DatabaseId)) throw new ArgumentNullException(nameof(Settings.DatabaseId));
            _containerCreator = new CosmosContainerCreator(CosmonautClient);
            _databaseCreator = new CosmosDatabaseCreator(CosmonautClient);
            InitialiseCosmosStore(overriddenCollectionName);
        }

        public CosmosStore(ICosmonautClient cosmonautClient,
            string databaseId) : this(cosmonautClient, databaseId, string.Empty,
            new CosmosDatabaseCreator(cosmonautClient),
            new CosmosContainerCreator(cosmonautClient))
        {
        }

        public CosmosStore(ICosmonautClient cosmonautClient,
            string databaseId,
            string overriddenCollectionName) : this(cosmonautClient,
            databaseId,
            overriddenCollectionName,
            new CosmosDatabaseCreator(cosmonautClient),
            new CosmosContainerCreator(cosmonautClient))
        {
        }

        internal CosmosStore(ICosmonautClient cosmonautClient,
            string databaseId,
            string overriddenCollectionName,
            IDatabaseCreator databaseCreator = null,
            IContainerCreator containerCreator = null)
        {
            CosmonautClient = cosmonautClient ?? throw new ArgumentNullException(nameof(cosmonautClient));
            Settings = new CosmosStoreSettings(databaseId, cosmonautClient.CosmosClient.Endpoint.ToString(), string.Empty, cosmonautClient.CosmosClient.ClientOptions.ConnectionMode);
            
            Database = CosmonautClient.CosmosClient.GetDatabase(databaseId);
            if (Settings.InfiniteRetries)
                CosmonautClient.CosmosClient.SetupInfiniteRetries();
            if (string.IsNullOrEmpty(Settings.DatabaseId)) throw new ArgumentNullException(nameof(Settings.DatabaseId));
            _containerCreator = containerCreator ?? new CosmosContainerCreator(CosmonautClient);
            _databaseCreator = databaseCreator ?? new CosmosDatabaseCreator(CosmonautClient);
            InitialiseCosmosStore(overriddenCollectionName);
        }

        public IQueryable<TEntity> Query(QueryRequestOptions requestOptions = null, string continuationToken = null, 
            bool allowSynchronousQueryExecution = false)
        {
            var queryable = Container.GetItemLinqQueryable<TEntity>(allowSynchronousQueryExecution, continuationToken, requestOptions);
            return IsShared ? queryable.Where(ExpressionExtensions.SharedCollectionExpression<TEntity>()) : queryable;
        }

        public FeedIterator<TEntity> Query(string sql, object parameters = null, QueryRequestOptions queryRequestOptions = null, string continuationToken = null,
            CancellationToken cancellationToken = default)
        {
            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(collectionSharingFriendlySql);
            var dictionary = parameters.ConvertToSqlParameterDictionary();
            queryDefinition = dictionary.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            return Container.GetItemQueryIterator<TEntity>(queryDefinition, continuationToken, queryRequestOptions);
        }

//        public async Task<TEntity> QuerySingleAsync(string sql, object parameters = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
//        {
//            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();
//            var queryable = CosmonautClient.Query<TEntity>(DatabaseName, CollectionName, collectionSharingFriendlySql, parameters, GetFeedOptionsForQuery(feedOptions));
//            return await queryable.SingleOrDefaultAsync(cancellationToken);
//        }
//
//        public async Task<T> QuerySingleAsync<T>(string sql, object parameters = null, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
//        {
//            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();
//            var queryable = CosmonautClient.Query<T>(DatabaseName, CollectionName, collectionSharingFriendlySql, parameters, GetFeedOptionsForQuery(feedOptions));
//            return await queryable.SingleOrDefaultAsync(cancellationToken);
//        }
        
        public async Task<IEnumerable<TEntity>> QueryMultipleAsync(string sql, object parameters = null, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(collectionSharingFriendlySql);
            var dictionary = parameters.ConvertToSqlParameterDictionary();
            queryDefinition = dictionary.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            var iterator = Container.GetItemQueryIterator<TEntity>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> QueryMultipleAsync<T>(string sql, object parameters = null, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(collectionSharingFriendlySql);
            var dictionary = parameters.ConvertToSqlParameterDictionary();
            queryDefinition = dictionary.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            var iterator = Container.GetItemQueryIterator<T>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.ToListAsync(cancellationToken);
        }

        public FeedIterator<TEntity> Query(string sql, IDictionary<string, object> parameters, QueryRequestOptions queryRequestOptions = null, string continuationToken = null,
            CancellationToken cancellationToken = default)
        {
            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(collectionSharingFriendlySql);
            //TODO make this @ symbol safe
            queryDefinition = parameters.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            return Container.GetItemQueryIterator<TEntity>(queryDefinition, continuationToken, queryRequestOptions);
        }
//
//        public async Task<TEntity> QuerySingleAsync(string sql, IDictionary<string, object> parameters, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
//        {
//            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();
//            var queryable = CosmonautClient.Query<TEntity>(DatabaseName, CollectionName, collectionSharingFriendlySql, parameters, GetFeedOptionsForQuery(feedOptions));
//            return await queryable.SingleOrDefaultAsync(cancellationToken);
//        }
//
//        public async Task<T> QuerySingleAsync<T>(string sql, IDictionary<string, object> parameters, FeedOptions feedOptions = null, CancellationToken cancellationToken = default)
//        {
//            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();
//            var queryable = CosmonautClient.Query<T>(DatabaseName, CollectionName, collectionSharingFriendlySql, parameters, GetFeedOptionsForQuery(feedOptions));
//            return await queryable.SingleOrDefaultAsync(cancellationToken);
//        }
//
        public async Task<IEnumerable<TEntity>> QueryMultipleAsync(string sql, IDictionary<string, object> parameters, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(collectionSharingFriendlySql);
            queryDefinition = parameters.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            var iterator = Container.GetItemQueryIterator<TEntity>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> QueryMultipleAsync<T>(string sql, IDictionary<string, object> parameters, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var collectionSharingFriendlySql = sql.EnsureQueryIsCollectionSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(collectionSharingFriendlySql);
            queryDefinition = parameters.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            var iterator = Container.GetItemQueryIterator<T>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.ToListAsync(cancellationToken);
        }

        public async Task<CosmosResponse<TEntity>> AddAsync(TEntity entity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            entity.ValidateEntityForCosmosDb();
            var stream = entity.ToCosmonautStream(Settings.CosmosSerializer);
            var response = await Container.CreateItemStreamAsync(stream, entity.GetPartitionKeyValueForEntity(), requestOptions, cancellationToken);
            return new CosmosResponse<TEntity>(Settings.CosmosSerializer, response);
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, ItemRequestOptions> requestOptions = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteMultiOperationAsync(entities, x => AddAsync(x, requestOptions?.Invoke(x), cancellationToken));
        }
//        
        public async Task<CosmosMultipleResponse<TEntity>> RemoveAsync(
            Expression<Func<TEntity, bool>> predicate, 
            QueryRequestOptions queryRequestOptions = null,
            Func<TEntity, ItemRequestOptions> requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var entitiesToRemove = await Query(queryRequestOptions).Where(predicate).ToListAsync(cancellationToken);
            return await RemoveRangeAsync(entitiesToRemove, requestOptions, cancellationToken);
        }

        public async Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            entity.ValidateEntityForCosmosDb();
            var documentId = entity.GetDocumentId();
            var response = await Container.DeleteItemStreamAsync(documentId, entity.GetPartitionKeyValueForEntity(), requestOptions, cancellationToken);//.ExecuteCosmosCommand(entity);
            return new CosmosResponse<TEntity>(Settings.CosmosSerializer, response);
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, ItemRequestOptions> requestOptions = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteMultiOperationAsync(entities, x => RemoveAsync(x, requestOptions?.Invoke(x), cancellationToken));
        }
//
//        public async Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
//        {
//            entity.ValidateEntityForCosmosDb();
//            requestOptions = GetRequestOptions(requestOptions, entity);
//            var document = entity.ToCosmonautDocument(requestOptions?.JsonSerializerSettings ?? Settings.JsonSerializerSettings);
//            return await CosmonautClient.UpdateDocumentAsync(DatabaseName, CollectionName, document,
//                requestOptions, cancellationToken).ExecuteCosmosCommand(entity);
//        }
//        
//        public async Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, RequestOptions> requestOptions = null, CancellationToken cancellationToken = default)
//        {
//            return await ExecuteMultiOperationAsync(entities, x => UpdateAsync(x, requestOptions?.Invoke(x), cancellationToken));
//        }
//
//        public async Task<CosmosResponse<TEntity>> UpsertAsync(TEntity entity, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
//        {
//            requestOptions = GetRequestOptions(requestOptions, entity);
//            var document = entity.ToCosmonautDocument(requestOptions?.JsonSerializerSettings ?? Settings.JsonSerializerSettings);
//            return await CosmonautClient.UpsertDocumentAsync(DatabaseName, CollectionName, document,
//                requestOptions, cancellationToken).ExecuteCosmosCommand(entity);
//        }
//
//        public async Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, RequestOptions> requestOptions = null, CancellationToken cancellationToken = default)
//        {
//            return await ExecuteMultiOperationAsync(entities, x => UpsertAsync(x, requestOptions?.Invoke(x), cancellationToken));
//        }
//        
//        public async Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
//        {
//            var response = await CosmonautClient.DeleteDocumentAsync(DatabaseName, CollectionName, id,
//                GetRequestOptions(id, requestOptions), cancellationToken);
//            return new CosmosResponse<TEntity>(response);
//        }
//
//        public async Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id, object partitionKeyValue, CancellationToken cancellationToken = default)
//        {
//            var requestOptions = partitionKeyValue != null
//                ? new RequestOptions { PartitionKey = new PartitionKey(partitionKeyValue) }
//                : null;
//
//            return await RemoveByIdAsync(id, requestOptions, cancellationToken);
//        }
//
//        public async Task<TEntity> FindAsync(string id, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
//        {
//            return await CosmonautClient.GetDocumentAsync<TEntity>(DatabaseName, CollectionName, id,
//                GetRequestOptions(id, requestOptions), cancellationToken);
//        }
//
//        public async Task<TEntity> FindAsync(string id, object partitionKeyValue, CancellationToken cancellationToken = default)
//        {
//            var requestOptions = partitionKeyValue != null
//                ? new RequestOptions { PartitionKey = new PartitionKey(partitionKeyValue) }
//                : null;
//            return await FindAsync(id, requestOptions, cancellationToken);
//        }
//
        public async Task<bool> EnsureInfrastructureProvisionedAsync()
        {
            var databaseCreated =
                await _databaseCreator.EnsureCreatedAsync(Database.Id, Settings.DefaultDatabaseThroughput);
            var collectionCreated = await _containerCreator.EnsureCreatedAsync<TEntity>(Database.Id, Container.Id,
                Settings.DefaultContainerThroughput, Settings.CosmosSerializer, Settings.IndexingPolicy, Settings.OnDatabaseThroughput, Settings.UniqueKeyPolicy);

            return databaseCreated && collectionCreated;
        }
        
        private void InitialiseCosmosStore(string overridenCollectionName)
        {
            IsShared = typeof(TEntity).UsesSharedCollection();
            Container = CosmonautClient.CosmosClient.GetContainer(Database.Id, GetCosmosStoreContainerName(overridenCollectionName));

            if (Settings.ProvisionInfrastructureIfMissing)
            {
                EnsureInfrastructureProvisionedAsync().GetAwaiter().GetResult();
            }

            Settings.CosmosSerializer = CosmonautClient.CosmosClient.ClientOptions.Serializer;
        }

        private string GetCosmosStoreContainerName(string overridenContainerName)
        {
            var hasOverridenName = !string.IsNullOrEmpty(overridenContainerName);
            return IsShared
                ? $"{Settings.CollectionPrefix ?? string.Empty}{(hasOverridenName ? overridenContainerName : typeof(TEntity).GetSharedCollectionName())}"
                : $"{Settings.CollectionPrefix ?? string.Empty}{(hasOverridenName ? overridenContainerName : typeof(TEntity).GetCollectionName())}";
        }

        private async Task<CosmosMultipleResponse<TEntity>> ExecuteMultiOperationAsync(IEnumerable<TEntity> entities,
            Func<TEntity, Task<CosmosResponse<TEntity>>> operationFunc)
        {
            var multipleResponse = new CosmosMultipleResponse<TEntity>();

            var entitiesList = entities.ToList();
            if (!entitiesList.Any())
                return multipleResponse;
            
            var results = (await entitiesList.Select(operationFunc).WhenAllTasksAsync()).ToList();
            multipleResponse.SuccessfulEntities.AddRange(results.Where(x => x.IsSuccess));
            multipleResponse.FailedEntities.AddRange(results.Where(x => !x.IsSuccess));
            return multipleResponse;
        }

        private QueryRequestOptions GetQueryRequestOptions(string id, QueryRequestOptions requestOptions)
        {
            var partitionKeyDefinition = typeof(TEntity).GetPartitionKeyDefinitionForEntity(Settings.CosmosSerializer);
            var partitionKeyIsId = partitionKeyDefinition?.Equals($"/{CosmosConstants.CosmosId}") ?? false;
            if (requestOptions == null && partitionKeyIsId)
            {
                return new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(id)
                };
            }

            if (requestOptions != null && partitionKeyIsId)
                requestOptions.PartitionKey = new PartitionKey(id);

            return requestOptions;
        }
    }
}