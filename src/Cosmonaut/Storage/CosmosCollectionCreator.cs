using System;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Storage
{
    internal class CosmosCollectionCreator : ICollectionCreator
    {
        private readonly ICosmonautClient _cosmonautClient;

        public CosmosCollectionCreator(ICosmonautClient cosmonautClient)
        {
            _cosmonautClient = cosmonautClient;
        }

        public CosmosCollectionCreator(IDocumentClient documentClient)
        {
            _cosmonautClient = new CosmonautClient(documentClient);
        }

        public async Task<bool> EnsureCreatedAsync<TEntity>(
            string databaseId,
            string collectionId,
            int collectionThroughput,
            IndexingPolicy indexingPolicy = null) where TEntity : class
        {
            var collectionResource = await _cosmonautClient.GetCollectionAsync(databaseId, collectionId);

            if (collectionResource != null)
                return true;

            var newCollection = new DocumentCollection
            {
                Id = collectionId,
                IndexingPolicy = indexingPolicy ?? CosmosConstants.DefaultIndexingPolicy
            };

            SetPartitionKeyDefinitionForCollection(typeof(TEntity), newCollection);

            newCollection = await _cosmonautClient.CreateCollectionAsync(databaseId, newCollection, new RequestOptions
            {
                OfferThroughput = collectionThroughput
            });

            return newCollection != null;
        }

        private static void SetPartitionKeyDefinitionForCollection(Type entityType, DocumentCollection collection)
        {
            var partitionKey = entityType.GetPartitionKeyDefinitionForEntity();

            if (partitionKey != null)
                collection.PartitionKey = partitionKey;
        }
    }
}