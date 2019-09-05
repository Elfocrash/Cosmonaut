using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Factories;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut
{
    public class CosmonautClient : ICosmonautClient
    {
        private readonly CosmosSerializer _serializerSettings;
        
        public CosmonautClient(CosmosClient cosmosClient, bool infiniteRetrying = true)
        {
            CosmosClient = cosmosClient;
            if (infiniteRetrying)
                CosmosClient.SetupInfiniteRetries();

            _serializerSettings = CosmosClient.ClientOptions.Serializer;
        }
        
        public CosmonautClient(Func<CosmosClient> cosmosClientFunc, bool infiniteRetrying = true)
        {
            CosmosClient = cosmosClientFunc();
            if (infiniteRetrying)
                CosmosClient.SetupInfiniteRetries();
            
            _serializerSettings = CosmosClient.ClientOptions.Serializer;
        }

        public CosmonautClient(
            Uri endpoint, 
            string authKeyOrResourceToken, 
            CosmosClientOptions clientOptions = null,
            bool infiniteRetrying = true)
        {
            CosmosClient = new CosmosClient(endpoint.ToString(), authKeyOrResourceToken, clientOptions);
            if (infiniteRetrying)
                CosmosClient.SetupInfiniteRetries();

            _serializerSettings = CosmosClient.ClientOptions.Serializer;
        }

        public CosmonautClient(
            string endpoint,
            string authKeyOrResourceToken,
            CosmosClientOptions clientOptions = null,
            bool infiniteRetrying = true) : this(new Uri(endpoint), authKeyOrResourceToken, clientOptions, infiniteRetrying)
        {
        }

        public async Task<DatabaseResponse> GetDatabaseAsync(string databaseId, RequestOptions requestOptions = null)
        {
            return await CosmosClient.CreateDatabaseAsync(databaseId);
        }

        public Task<IEnumerable<DatabaseResponse>> QueryDatabasesAsync(Expression<Func<DatabaseResponse, bool>> predicate = null, RequestOptions requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ContainerResponse>> QueryContainersAsync(string databaseId, Expression<Func<ContainerResponse, bool>> predicate = null, ContainerRequestOptions requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryItemsAsync<T>(string databaseId, string collectionId, Expression<Func<T, bool>> predicate = null,
            ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryItemsAsync<T>(string databaseId, string collectionId, string sql, object parameters = null,
            ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryItemsAsync<T>(string databaseId, string collectionId, string sql, IDictionary<string, object> parameters,
            ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetItemAsync<T>(string databaseId, string collectionId, string documentId,
            ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<ContainerResponse> GetContainerAsync(string databaseId, string containerId, ContainerRequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Query<T>(string databaseId, string collectionId, ItemRequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Query<T>(string databaseId, string collectionId, string sql, object parameters = null,
            ItemRequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }
        
        public CosmosClient CosmosClient { get; }
    }
}