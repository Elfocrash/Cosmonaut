using System;
using System.Net;
using System.Threading.Tasks;
using Cosmonaut.Configuration;
using Cosmonaut.Extensions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Cosmonaut.Storage
{
    internal class CosmosContainerCreator : IContainerCreator
    {
        private readonly ICosmonautClient _cosmonautClient;

        public CosmosContainerCreator(ICosmonautClient cosmonautClient)
        {
            _cosmonautClient = cosmonautClient;
        }

        public CosmosContainerCreator(CosmosClient documentClient)
        {
            _cosmonautClient = new CosmonautClient(documentClient);
        }

        public async Task<bool> EnsureCreatedAsync<TEntity>(
            string databaseId,
            string containerId,
            int containerThroughput,
            JsonSerializerSettings partitionKeySerializer,
            IndexingPolicy indexingPolicy = null,
            ThroughputBehaviour onDatabaseBehaviour = ThroughputBehaviour.UseDatabaseThroughput, 
            UniqueKeyPolicy uniqueKeyPolicy = null) where TEntity : class
        {
            var containerResponse = await _cosmonautClient.GetContainerAsync(databaseId, containerId);
            var databaseHasOffer = await _cosmonautClient.CosmosClient.GetDatabase(databaseId).ReadThroughputAsync() != null;

            if (containerResponse.StatusCode != HttpStatusCode.NotFound)
                return true;

            var partitionKeyDef = typeof(TEntity).GetPartitionKeyDefinitionForEntity(partitionKeySerializer);
            
            var containerProperties = new ContainerProperties
            {
                Id = containerId,
                IndexingPolicy = indexingPolicy ?? CosmosConstants.DefaultIndexingPolicy,
                UniqueKeyPolicy = uniqueKeyPolicy ?? CosmosConstants.DefaultUniqueKeyPolicy,
                PartitionKeyPath = partitionKeyDef
            };

            var finalCollectionThroughput = databaseHasOffer ? onDatabaseBehaviour == ThroughputBehaviour.DedicateCollectionThroughput ? (int?)containerThroughput : null : containerThroughput;

            var response = await _cosmonautClient.CosmosClient.GetDatabase(databaseId).CreateContainerAsync(containerProperties, finalCollectionThroughput);

            return response != null; // TODO check for status code
        }
    }
}