using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Storage
{
    public interface ICollectionCreator
    {
        Task<bool> EnsureCreatedAsync(Type entityType, Database database, int collectionThroughput);
    }
}