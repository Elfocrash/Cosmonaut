using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Cosmonaut.Factories
{
    public class DocumentClientFactory
    {
        public static IDocumentClient CreateDocumentClient(CosmosStoreSettings settings)
        {
            return new DocumentClient(settings.EndpointUrl, settings.AuthKey, settings.JsonSerializerSettings, settings.ConnectionPolicy ?? ConnectionPolicy.Default, settings.ConsistencyLevel);
        }

        internal static IDocumentClient CreateDocumentClient(Uri endpoint, string authKeyOrResourceToken, ConnectionPolicy connectionPolicy = null, ConsistencyLevel? desiredConsistencyLevel = null)
        {
            return new DocumentClient(endpoint, authKeyOrResourceToken, connectionPolicy ?? ConnectionPolicy.Default, desiredConsistencyLevel);
        }

        internal static IDocumentClient CreateDocumentClient(Uri endpoint, string authKeyOrResourceToken, JsonSerializerSettings jsonSerializerSettings, ConnectionPolicy connectionPolicy = null, ConsistencyLevel? desiredConsistencyLevel = null)
        {
            return new DocumentClient(endpoint, authKeyOrResourceToken, jsonSerializerSettings, connectionPolicy ?? ConnectionPolicy.Default, desiredConsistencyLevel);
        }
    }
}