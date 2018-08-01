using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public class DocumentClientFactory
    {
        public static IDocumentClient CreateDocumentClient(string endpointUrl, string authKey, ConnectionPolicy connectionPolicy = null, ConsistencyLevel? desiredConsistencyLevel = null)
        {
            return CreateDocumentClient(new Uri(endpointUrl), authKey, connectionPolicy, desiredConsistencyLevel);
        }

        public static IDocumentClient CreateDocumentClient(Uri endpointUrl, string authKey, ConnectionPolicy connectionPolicy = null, ConsistencyLevel? desiredConsistencyLevel = null)
        {
            return new DocumentClient(endpointUrl, authKey, connectionPolicy ?? ConnectionPolicy.Default, desiredConsistencyLevel);
        }

        public static IDocumentClient CreateDocumentClient(CosmosStoreSettings settings)
        {
            return new DocumentClient(settings.EndpointUrl, settings.AuthKey, settings.ConnectionPolicy ?? ConnectionPolicy.Default, settings.ConsistencyLevel);
        }
    }
}