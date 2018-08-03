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
            var isSharedCollection = typeof(TEntity).UsesSharedCollection();

            var collectionResource = await _cosmonautClient.GetCollectionAsync(databaseId, collectionId);

            if (collectionResource != null)
                return true;

            var newCollection = new DocumentCollection
            {
                Id = collectionId
            };

            SetPartitionKeyIfCollectionIsNotShared(typeof(TEntity), isSharedCollection, newCollection);
            SetPartitionKeyAsIdIfCollectionIsShared(isSharedCollection, newCollection);

            if (indexingPolicy != null)
                newCollection.IndexingPolicy = indexingPolicy;

            newCollection = await _cosmonautClient.CreateCollectionAsync(newCollection, databaseId, new RequestOptions
            {
                OfferThroughput = collectionThroughput
            });

            return newCollection != null;
        }

        private static void SetPartitionKeyAsIdIfCollectionIsShared(bool isSharedCollection, DocumentCollection collection)
        {
            if (isSharedCollection)
            {
                collection.PartitionKey = DocumentHelpers.GetPartitionKeyDefinition(CosmosConstants.CosmosId);
            }
        }

        private static void SetPartitionKeyIfCollectionIsNotShared(Type entityType, bool isSharedCollection, DocumentCollection collection)
        {
            if (isSharedCollection) return;
            var partitionKey = entityType.GetPartitionKeyForEntity();

            if (partitionKey != null)
                collection.PartitionKey = partitionKey;
        }
    }
}