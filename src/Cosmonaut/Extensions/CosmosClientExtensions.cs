using Microsoft.Azure.Cosmos;

namespace Cosmonaut.Extensions
{
    public static class CosmosClientExtensions
    {
        public static void SetupInfiniteRetries(this CosmosClient cosmosClient)
        {
            cosmosClient.ClientOptions.MaxRetryAttemptsOnRateLimitedRequests = int.MaxValue;
        }
    }
}