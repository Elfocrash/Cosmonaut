using System;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public class CosmosStoreSettings
    {
        public readonly string DatabaseName;

        public readonly string AuthKey;

        public readonly Uri EndpointUrl;

        public readonly ConnectionPolicy ConnectionPolicy;

        public CosmosStoreSettings(string databaseName, string endpointUrl, string authKey, ConnectionPolicy connectionPolicy = null)
        {
            DatabaseName = databaseName;
            AuthKey = authKey;
            EndpointUrl = new Uri(endpointUrl);
            ConnectionPolicy = connectionPolicy;
        }

        public CosmosStoreSettings(string databaseName, Uri endpointUrl, string authKey, ConnectionPolicy connectionPolicy = null)
        {
            DatabaseName = databaseName;
            AuthKey = authKey;
            EndpointUrl = endpointUrl;
            ConnectionPolicy = connectionPolicy;
        }
    }
}