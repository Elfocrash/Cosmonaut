using System;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Cosmonaut.Factories
{
    public class CosmosClientFactory
    {
        public static CosmosClient CreateCosmosClient(CosmosStoreSettings settings)
        {
            return new CosmosClient(settings.EndpointUrl.ToString(), settings.AuthKey, new CosmosClientOptions
                {
                    Serializer = settings.CosmosSerializer,
                    ConnectionMode = settings.ConnectionMode,
                    ConsistencyLevel = settings.ConsistencyLevel
                });
        }
    }
}