using System.Threading.Tasks;
using Cosmonaut.StoredProcedures;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.Storage
{
    public class CosmosScriptCreator : IScriptCreator
    {
        private readonly ICosmonautClient _cosmonautClient;

        public CosmosScriptCreator(ICosmonautClient cosmonautClient)
        {
            _cosmonautClient = cosmonautClient;
        }

        public CosmosScriptCreator(IDocumentClient documentClient)
        {
            _cosmonautClient = new CosmonautClient(documentClient);
        }

        public async Task<bool> EnsureCreatedAsync(string databaseId, string collectionId)
        {
            foreach (var storedProcedure in CosmonautStoredProcedures.Values)
            {
                var exists = await _cosmonautClient.GetStoredProcedureAsync(databaseId, collectionId, storedProcedure.Id);
                if (exists == null)
                {
                    await _cosmonautClient.DocumentClient.CreateStoredProcedureAsync(
                        UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), storedProcedure);
                }
            }

            return true;
        }
    }
}