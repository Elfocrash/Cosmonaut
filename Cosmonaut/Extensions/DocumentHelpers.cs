using Microsoft.Azure.Documents;

namespace Cosmonaut.Extensions
{
    internal class DocumentHelpers
    {
        internal static PartitionKeyDefinition GetPartitionKeyDefinition(string partitionKeyName)
        {
            return new PartitionKeyDefinition
            {
                Paths =
                {
                    $"/{partitionKeyName}"
                }
            };
        }

        internal static string GetDocumentSelfLink(string databaseName, string collectionName, string documentId) =>
            $"dbs/{databaseName}/colls/{collectionName}/docs/{documentId}/";
    }
}