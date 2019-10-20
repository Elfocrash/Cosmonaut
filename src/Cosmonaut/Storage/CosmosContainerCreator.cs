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
            CosmosSerializer partitionKeySerializer,
            IndexingPolicy indexingPolicy = null,
            ThroughputBehaviour onDatabaseBehaviour = ThroughputBehaviour.UseDatabaseThroughput, 
            UniqueKeyPolicy uniqueKeyPolicy = null) where TEntity : class
        {
            var containerResponse = await _cosmonautClient.CosmosClient.GetContainer(databaseId, containerId).ReadContainerAsync();
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

            var finalContainerThroughput = databaseHasOffer ? onDatabaseBehaviour == ThroughputBehaviour.DedicateContainerThroughput ? (int?)containerThroughput : null : containerThroughput;

            var response = await _cosmonautClient.CosmosClient.GetDatabase(databaseId).CreateContainerAsync(containerProperties, finalContainerThroughput);

            return response != null; // TODO check for status code
        }
    }
}