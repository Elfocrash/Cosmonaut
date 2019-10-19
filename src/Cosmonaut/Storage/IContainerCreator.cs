using System.Threading.Tasks;
using Cosmonaut.Configuration;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Cosmonaut.Storage
{
    public interface IContainerCreator
    {
        Task<bool> EnsureCreatedAsync<TEntity>(string databaseId, string containerId, int containerThroughput, CosmosSerializer partitionKeySerializer, IndexingPolicy indexingPolicy = null, ThroughputBehaviour onDatabaseBehaviour = ThroughputBehaviour.UseDatabaseThroughput, UniqueKeyPolicy uniqueKeyPolicy = null) where TEntity : class;
    }
}