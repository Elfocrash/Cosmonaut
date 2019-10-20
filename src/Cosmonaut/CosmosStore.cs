
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

        public CosmosStore(CosmosStoreSettings settings, string overriddenContainerName)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            var documentClient = CosmosClientFactory.CreateCosmosClient(settings);
            CosmonautClient = new CosmonautClient(documentClient, Settings.InfiniteRetries);
            Database = CosmonautClient.CosmosClient.GetDatabase(settings.DatabaseId);
            
            if (string.IsNullOrEmpty(Settings.DatabaseId)) throw new ArgumentNullException(nameof(Settings.DatabaseId));
            _containerCreator = new CosmosContainerCreator(CosmonautClient);
            _databaseCreator = new CosmosDatabaseCreator(CosmonautClient);
            InitialiseCosmosStore(overriddenContainerName);
        }

        public CosmosStore(ICosmonautClient cosmonautClient,
            string databaseId) : this(cosmonautClient, databaseId, string.Empty,
            new CosmosDatabaseCreator(cosmonautClient),
            new CosmosContainerCreator(cosmonautClient))
        {
        }

        public CosmosStore(ICosmonautClient cosmonautClient,
            string databaseId,
            string overriddenContainerName) : this(cosmonautClient,
            databaseId,
            overriddenContainerName,
            new CosmosDatabaseCreator(cosmonautClient),
            new CosmosContainerCreator(cosmonautClient))
        {
        }

        internal CosmosStore(ICosmonautClient cosmonautClient,
            string databaseId,
            string overriddenContainerName,
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
            InitialiseCosmosStore(overriddenContainerName);
        }

        public IQueryable<TEntity> Query(QueryRequestOptions requestOptions = null, string continuationToken = null, 
            bool allowSynchronousQueryExecution = false)
        {
            var queryable = Container.GetItemLinqQueryable<TEntity>(allowSynchronousQueryExecution, continuationToken, requestOptions);
            return IsShared ? queryable.Where(ExpressionExtensions.SharedContainerExpression<TEntity>()) : queryable;
        }

        public FeedIterator<TEntity> Query(string sql, object parameters = null, QueryRequestOptions queryRequestOptions = null, string continuationToken = null,
            CancellationToken cancellationToken = default)
        {
            var containerSharingFriendlySql = sql.EnsureQueryIsContainerSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(containerSharingFriendlySql);
            var dictionary = parameters.ConvertToSqlParameterDictionary();
            queryDefinition = dictionary.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            return Container.GetItemQueryIterator<TEntity>(queryDefinition, continuationToken, queryRequestOptions);
        }

        public async Task<TEntity> QuerySingleAsync(string sql, object parameters = null, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var containerSharingFriendlySql = sql.EnsureQueryIsContainerSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(containerSharingFriendlySql);
            var dictionary = parameters.ConvertToSqlParameterDictionary();
            queryDefinition = dictionary.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            PrepareQueryRequestOptionsForSingleOperation(queryRequestOptions);
            var iterator = Container.GetItemQueryIterator<TEntity>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<T> QuerySingleAsync<T>(string sql, object parameters = null, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var containerSharingFriendlySql = sql.EnsureQueryIsContainerSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(containerSharingFriendlySql);
            var dictionary = parameters.ConvertToSqlParameterDictionary();
            queryDefinition = dictionary.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            PrepareQueryRequestOptionsForSingleOperation(queryRequestOptions);
            var iterator = Container.GetItemQueryIterator<T>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.SingleOrDefaultAsync(cancellationToken);
        }
        
        public async Task<IEnumerable<TEntity>> QueryMultipleAsync(string sql, object parameters = null, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var containerSharingFriendlySql = sql.EnsureQueryIsContainerSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(containerSharingFriendlySql);
            var dictionary = parameters.ConvertToSqlParameterDictionary();
            queryDefinition = dictionary.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            var iterator = Container.GetItemQueryIterator<TEntity>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> QueryMultipleAsync<T>(string sql, object parameters = null, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var containerSharingFriendlySql = sql.EnsureQueryIsContainerSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(containerSharingFriendlySql);
            var dictionary = parameters.ConvertToSqlParameterDictionary();
            queryDefinition = dictionary.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            var iterator = Container.GetItemQueryIterator<T>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.ToListAsync(cancellationToken);
        }

        public FeedIterator<TEntity> Query(string sql, IDictionary<string, object> parameters, QueryRequestOptions queryRequestOptions = null, string continuationToken = null,
            CancellationToken cancellationToken = default)
        {
            var containerSharingFriendlySql = sql.EnsureQueryIsContainerSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(containerSharingFriendlySql);
            queryDefinition = parameters.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            return Container.GetItemQueryIterator<TEntity>(queryDefinition, continuationToken, queryRequestOptions);
        }

        public async Task<TEntity> QuerySingleAsync(string sql, IDictionary<string, object> parameters, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var containerSharingFriendlySql = sql.EnsureQueryIsContainerSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(containerSharingFriendlySql);
            queryDefinition = parameters.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            var iterator = Container.GetItemQueryIterator<TEntity>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<T> QuerySingleAsync<T>(string sql, IDictionary<string, object> parameters, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var containerSharingFriendlySql = sql.EnsureQueryIsContainerSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(containerSharingFriendlySql);
            var iterator = Container.GetItemQueryIterator<T>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> QueryMultipleAsync(string sql, IDictionary<string, object> parameters, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var containerSharingFriendlySql = sql.EnsureQueryIsContainerSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(containerSharingFriendlySql);
            queryDefinition = parameters.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            var iterator = Container.GetItemQueryIterator<TEntity>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> QueryMultipleAsync<T>(string sql, IDictionary<string, object> parameters, QueryRequestOptions queryRequestOptions = null, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            var containerSharingFriendlySql = sql.EnsureQueryIsContainerSharingFriendly<TEntity>();
            var queryDefinition = new QueryDefinition(containerSharingFriendlySql);
            queryDefinition = parameters.Aggregate(queryDefinition, (current, parameter) => current.WithParameter($"@{parameter.Key}", parameter.Value));
            var iterator = Container.GetItemQueryIterator<T>(queryDefinition, continuationToken, queryRequestOptions);
            return await iterator.ToListAsync(cancellationToken);
        }

        public async Task<CosmosResponse<TEntity>> AddAsync(TEntity entity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var stream = entity.ToCosmonautStream(Settings.CosmosSerializer);
            var response = await Container.CreateItemStreamAsync(stream, entity.GetPartitionKeyValueForEntity(), requestOptions, cancellationToken);
            return new CosmosResponse<TEntity>(Settings.CosmosSerializer, response);
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, ItemRequestOptions> requestOptions = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteMultiOperationAsync(entities, x => AddAsync(x, requestOptions?.Invoke(x), cancellationToken));
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> RemoveAsync(
            Expression<Func<TEntity, bool>> predicate, 
            QueryRequestOptions queryRequestOptions = null,
            Func<TEntity, ItemRequestOptions> requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var entitiesToRemove = await Query(queryRequestOptions).Where(predicate).ToListAsync(cancellationToken);
            return await RemoveRangeAsync(entitiesToRemove, requestOptions, cancellationToken);
        }
        
        public async Task<CosmosResponse<TEntity>> UpdateAsync(TEntity entity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var documentId = entity.GetDocumentId();
            var stream = entity.ToCosmonautStream(Settings.CosmosSerializer);
            var response = await Container.ReplaceItemStreamAsync(stream, documentId, entity.GetPartitionKeyValueForEntity(), requestOptions, cancellationToken);//.ExecuteCosmosCommand(entity);
            return new CosmosResponse<TEntity>(Settings.CosmosSerializer, response);
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, ItemRequestOptions> requestOptions = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteMultiOperationAsync(entities, x => UpdateAsync(x, requestOptions?.Invoke(x), cancellationToken));
        }

        public async Task<CosmosResponse<TEntity>> RemoveAsync(TEntity entity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var documentId = entity.GetDocumentId();
            var response = await Container.DeleteItemStreamAsync(documentId, entity.GetPartitionKeyValueForEntity(), requestOptions, cancellationToken);//.ExecuteCosmosCommand(entity);
            return new CosmosResponse<TEntity>(Settings.CosmosSerializer, response);
        }
        
        public async Task<CosmosMultipleResponse<TEntity>> RemoveRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, ItemRequestOptions> requestOptions = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteMultiOperationAsync(entities, x => RemoveAsync(x, requestOptions?.Invoke(x), cancellationToken));
        }

        public async Task<CosmosResponse<TEntity>> UpsertAsync(TEntity entity, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var stream = entity.ToCosmonautStream(Settings.CosmosSerializer);
            var response = await Container.UpsertItemStreamAsync(stream, entity.GetPartitionKeyValueForEntity(), requestOptions, cancellationToken);//.ExecuteCosmosCommand(entity);
            return new CosmosResponse<TEntity>(Settings.CosmosSerializer, response);
        }

        public async Task<CosmosMultipleResponse<TEntity>> UpsertRangeAsync(IEnumerable<TEntity> entities, Func<TEntity, ItemRequestOptions> requestOptions = null, CancellationToken cancellationToken = default)
        {
            return await ExecuteMultiOperationAsync(entities, x => UpsertAsync(x, requestOptions?.Invoke(x), cancellationToken));
        }
        
        public async Task<CosmosResponse<TEntity>> RemoveByIdAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var response = await Container.DeleteItemStreamAsync(id, partitionKey, requestOptions, cancellationToken);
            return new CosmosResponse<TEntity>(Settings.CosmosSerializer, response);
        }

        public async Task<TEntity> FindAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return await Container.ReadItemAsync<TEntity>(id, partitionKey, requestOptions, cancellationToken);
        }

        public async Task<bool> EnsureInfrastructureProvisionedAsync()
        {
            var databaseCreated =
                await _databaseCreator.EnsureCreatedAsync(Database.Id, Settings.DefaultDatabaseThroughput);
            var containerCreated = await _containerCreator.EnsureCreatedAsync<TEntity>(Database.Id, Container.Id,
                Settings.DefaultContainerThroughput, Settings.CosmosSerializer, Settings.IndexingPolicy, Settings.OnDatabaseThroughput, Settings.UniqueKeyPolicy);

            return databaseCreated && containerCreated;
        }
        
        private void InitialiseCosmosStore(string overridenContainerName)
        {
            IsShared = typeof(TEntity).UsesSharedContainer();
            Container = CosmonautClient.CosmosClient.GetContainer(Database.Id, GetCosmosStoreContainerName(overridenContainerName));

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
                ? $"{Settings.ContainerPrefix ?? string.Empty}{(hasOverridenName ? overridenContainerName : typeof(TEntity).GetSharedContainerName())}"
                : $"{Settings.ContainerPrefix ?? string.Empty}{(hasOverridenName ? overridenContainerName : typeof(TEntity).GetContainerName())}";
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
        
        private static void PrepareQueryRequestOptionsForSingleOperation(QueryRequestOptions queryRequestOptions)
        {
            if (queryRequestOptions == null)
                queryRequestOptions = new QueryRequestOptions();

            queryRequestOptions.MaxItemCount = 1;
        }
    }
}