using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.Factories;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

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
        
        public CosmosClient CosmosClient { get; }
    }
}