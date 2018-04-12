using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public class DocumentClientFactory
    {
        public static IDocumentClient CreateDocumentClient(Uri endpointUrl, string authKey, ConnectionPolicy connectionPolicy = null)
        {
            return new DocumentClient(endpointUrl, authKey, connectionPolicy ?? ConnectionPolicy.Default);
        }

        public static IDocumentClient CreateDocumentClient(CosmosStoreSettings settings)
        {
            return new DocumentClient(settings.EndpointUrl, settings.AuthKey, settings.ConnectionPolicy ?? ConnectionPolicy.Default);
        }
    }
}