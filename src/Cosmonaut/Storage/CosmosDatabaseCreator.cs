﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Cosmonaut.Storage
{
    internal class CosmosDatabaseCreator : IDatabaseCreator
    {
        private readonly ICosmonautClient _cosmonautClient;

        public CosmosDatabaseCreator(ICosmonautClient cosmonautClient)
        {
            _cosmonautClient = cosmonautClient;
        }

        public CosmosDatabaseCreator(CosmosClient documentClient)
        {
            _cosmonautClient = new CosmonautClient(documentClient);
        }

        public async Task<bool> EnsureCreatedAsync(string databaseId, int? databaseThroughput = null)
        {
            try
            {
                var database = await _cosmonautClient.CosmosClient.GetDatabase(databaseId).ReadAsync();
                if (database.StatusCode == HttpStatusCode.OK) 
                    return true;
                
                return false;
            }
            catch (CosmosException ex)
            {
                var database = await _cosmonautClient.CosmosClient.CreateDatabaseAsync(databaseId, databaseThroughput);
                return database.StatusCode == HttpStatusCode.Created;
            }
        }
    }
}