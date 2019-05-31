using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut.Configuration;
using Cosmonaut.Exceptions;
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
            IndexingPolicy indexingPolicy = null,
            ThroughputBehaviour onDatabaseBehaviour = ThroughputBehaviour.UseDatabaseThroughput, 
            UniqueKeyPolicy uniqueKeyPolicy = null) where TEntity : class
        {
            var collectionResource = await _cosmonautClient.GetCollectionAsync(databaseId, collectionId);
            var databaseHasOffer = (await _cosmonautClient.GetOfferV2ForDatabaseAsync(databaseId)) != null;

            if (collectionResource != null)
                return true;

            var newCollection = new DocumentCollection
            {
                Id = collectionId,
                IndexingPolicy = indexingPolicy ?? CosmosConstants.DefaultIndexingPolicy,
                UniqueKeyPolicy = uniqueKeyPolicy ?? CosmosConstants.DefaultUniqueKeyPolicy
            };

            SetPartitionKeyDefinitionForCollection(typeof(TEntity), newCollection);

            var finalCollectionThroughput = databaseHasOffer ? onDatabaseBehaviour == ThroughputBehaviour.DedicateCollectionThroughput ? (int?)collectionThroughput : null : collectionThroughput;

            newCollection = await _cosmonautClient.CreateCollectionAsync(databaseId, newCollection, new RequestOptions
            {
                OfferThroughput = finalCollectionThroughput
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