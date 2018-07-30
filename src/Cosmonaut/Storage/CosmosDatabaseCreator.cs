using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Storage
{
    public class CosmosDatabaseCreator : IDatabaseCreator
    {
        private readonly IDocumentClient _documentClient;

        public CosmosDatabaseCreator(IDocumentClient documentClient)
        {
            _documentClient = documentClient;
        }

        public async Task<bool> EnsureCreatedAsync(string databaseName)
        {
            var database = _documentClient.CreateDatabaseQuery()
                .Where(db => db.Id == databaseName)
                .ToArray()
                .FirstOrDefault();

            if (database != null) return false;

            database = await _documentClient.CreateDatabaseAsync(
                new Database { Id = databaseName });
            return database != null;
        }
    }
}