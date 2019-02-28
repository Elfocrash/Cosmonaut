using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Storage
{
    internal class CosmosDatabaseCreator : IDatabaseCreator
    {
        private readonly ICosmonautClient _cosmonautClient;

        public CosmosDatabaseCreator(ICosmonautClient cosmonautClient)
        {
            _cosmonautClient = cosmonautClient;
        }

        public CosmosDatabaseCreator(IDocumentClient documentClient)
        {
            _cosmonautClient = new CosmonautClient(documentClient);
        }

        public async Task<bool> EnsureCreatedAsync(string databaseId, int? databaseThroughput = null)
        {
            var database = await _cosmonautClient.GetDatabaseAsync(databaseId);

            if (database != null) return false;

            var newDatabase = new Database {Id = databaseId};

            database = await _cosmonautClient.CreateDatabaseAsync(newDatabase, new RequestOptions
            {
                OfferThroughput = databaseThroughput
            });
            return database != null;
        }
    }
}