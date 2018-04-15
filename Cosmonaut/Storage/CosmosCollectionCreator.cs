using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Storage
{
    internal class CosmosCollectionCreator<TEntity> : ICollectionCreator<TEntity> where TEntity : class
    {
        private readonly IDocumentClient _documentClient;

        public CosmosCollectionCreator(IDocumentClient documentClient)
        {
            _documentClient = documentClient;
        }

        public async Task<bool> EnsureCreatedAsync(Type entityType, 
            Database database, 
            int collectionThroughput)
        {
            var collectionName = typeof(TEntity).GetCollectionName();
            var collection = _documentClient
                .CreateDocumentCollectionQuery(database.SelfLink)
                .ToArray()
                .FirstOrDefault(c => c.Id == collectionName);

            if (collection != null)
                return true;

            collection = new DocumentCollection
            {
                Id = collectionName
            };
            var partitionKey = typeof(TEntity).GetPartitionKeyForEntity();

            if (partitionKey != null)
                collection.PartitionKey = partitionKey;

            collection = await _documentClient.CreateDocumentCollectionAsync(database.SelfLink, collection, new RequestOptions
            {
                OfferThroughput = collectionThroughput
            });

            return collection != null;
        }
    }
}