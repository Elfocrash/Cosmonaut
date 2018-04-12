using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Storage
{
    public interface ICollectionCreator<TEntity> where TEntity : class
    {
        Task<bool> EnsureCreatedAsync(Type entityType, Database database, int collectionThroughput);
    }
}