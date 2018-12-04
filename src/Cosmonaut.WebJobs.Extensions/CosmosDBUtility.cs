using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut.WebJobs.Extensions
{
    internal static class CosmosDBUtility
    {
        internal static IEnumerable<string> ParsePreferredLocations(string preferredRegions)
        {
            if (string.IsNullOrEmpty(preferredRegions))
            {
                return Enumerable.Empty<string>();
            }

            return preferredRegions
                .Split(',')
                .Select((region) => region.Trim())
                .Where((region) => !string.IsNullOrEmpty(region));
        }

        internal static async Task CreateDatabaseAndCollectionIfNotExistAsync(IDocumentClient documentClient, string databaseName, string collectionName, string partitionKey, int throughput)
        {
            await documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });

            await CreateDocumentCollectionIfNotExistsAsync(documentClient, databaseName, collectionName, partitionKey, throughput);
        }

        internal static ConnectionPolicy BuildConnectionPolicy(ConnectionMode? connectionMode, Protocol? protocol, string preferredLocations, bool useMultipleWriteLocations)
        {
            var connectionPolicy = new ConnectionPolicy();
            if (connectionMode.HasValue)
            {
                connectionPolicy.ConnectionMode = connectionMode.Value;
            }

            if (protocol.HasValue)
            {
                connectionPolicy.ConnectionProtocol = protocol.Value;
            }

            if (useMultipleWriteLocations)
            {
                connectionPolicy.UseMultipleWriteLocations = useMultipleWriteLocations;
            }

            foreach (var location in ParsePreferredLocations(preferredLocations))
            {
                connectionPolicy.PreferredLocations.Add(location);
            }

            return connectionPolicy;
        }

        private static async Task<DocumentCollection> CreateDocumentCollectionIfNotExistsAsync(IDocumentClient documentClient, string databaseName, string collectionName,
            string partitionKey, int throughput)
        {
            Uri databaseUri = UriFactory.CreateDatabaseUri(databaseName);

            DocumentCollection documentCollection = new DocumentCollection
            {
                Id = collectionName
            };

            if (!string.IsNullOrEmpty(partitionKey))
            {
                documentCollection.PartitionKey.Paths.Add(partitionKey);
            }
            
            RequestOptions collectionOptions = null;
            if (throughput != 0)
            {
                collectionOptions = new RequestOptions
                {
                    OfferThroughput = throughput
                };
            }

            return await documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseUri, documentCollection, collectionOptions);
        }
    }
}
