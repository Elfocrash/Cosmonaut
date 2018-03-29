using System;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public class CosmosStoreSettings
    {
        public const int DefaultCollectionThroughput = 400;

        public readonly string DatabaseName;

        public readonly string AuthKey;

        public readonly Uri EndpointUrl;

        public readonly ConnectionPolicy ConnectionPolicy;

        public readonly int CollectionThroughput;

        public CosmosStoreSettings(string databaseName, string endpointUrl, string authKey, ConnectionPolicy connectionPolicy = null, int? collectionThroughput = null)
        {
            DatabaseName = databaseName;
            AuthKey = authKey;
            EndpointUrl = new Uri(endpointUrl);
            ConnectionPolicy = connectionPolicy;
            CollectionThroughput = collectionThroughput ?? DefaultCollectionThroughput;
        }

        public CosmosStoreSettings(string databaseName, Uri endpointUrl, string authKey, ConnectionPolicy connectionPolicy = null, int? collectionThroughput = null)
        {
            DatabaseName = databaseName;
            AuthKey = authKey;
            EndpointUrl = endpointUrl;
            ConnectionPolicy = connectionPolicy;
            CollectionThroughput = collectionThroughput ?? DefaultCollectionThroughput;
        }
    }
}