using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Storage
{
    public class CosmosDatabaseCreator : IDatabaseCreator
    {
        private readonly ICosmonautClient _cosmonautClient;

        public CosmosDatabaseCreator(ICosmonautClient cosmonautClient)
        {
            _cosmonautClient = cosmonautClient;
        }

        [Obsolete("This constructor will be dropped. Please use the one using ICosmonautClient instead.")]
        public CosmosDatabaseCreator(IDocumentClient documentClient)
        {
            _cosmonautClient = new CosmonautClient(documentClient);
        }

        public async Task<bool> EnsureCreatedAsync(string databaseId)
        {
            var database = await _cosmonautClient.GetDatabaseAsync(databaseId);

            if (database != null) return false;

            var newDatabase = new Database {Id = databaseId};

            database = await _cosmonautClient.CreateDatabaseAsync(newDatabase);
            return database != null;
        }
    }
}