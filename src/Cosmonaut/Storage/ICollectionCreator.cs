using System.Threading.Tasks;
using Cosmonaut.Configuration;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Storage
{
    public interface ICollectionCreator
    {
        Task<bool> EnsureCreatedAsync<TEntity>(string databaseId, string collectionId, int collectionThroughput, IndexingPolicy indexingPolicy = null, ThroughputBehaviour onDatabaseBehaviour = ThroughputBehaviour.UseDatabaseThroughput, UniqueKeyPolicy uniqueKeyPolicy = null) where TEntity : class;
    }
}