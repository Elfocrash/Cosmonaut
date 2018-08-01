using System;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Storage
{
    public class CosmosCollectionCreator : ICollectionCreator
    {
        private readonly ICosmonautClient _cosmonautClient;

        public CosmosCollectionCreator(ICosmonautClient cosmonautClient)
        {
            _cosmonautClient = cosmonautClient;
        }

        [Obsolete("This constructor will be dropped. Please use the one using ICosmonautClient instead.")]
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

            var collection = await _cosmonautClient.GetCollectionAsync(databaseId, collectionId);

            if (collection != null)
                return true;

            collection = new DocumentCollection
            {
                Id = collectionId
            };

            SetPartitionKeyIfCollectionIsNotShared(typeof(TEntity), isSharedCollection, collection);
            SetPartitionKeyAsIdIfCollectionIsShared(isSharedCollection, collection);

            if (indexingPolicy != null)
                collection.IndexingPolicy = indexingPolicy;
            
            collection = await _cosmonautClient.CreateCollectionAsync(collection, databaseId, new RequestOptions
            {
                OfferThroughput = collectionThroughput
            });

            return collection != null;
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