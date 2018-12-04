using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Cosmonaut
{
    public class CosmosStoreSettings
    {
        public string DatabaseName { get; }

        internal string AuthKey { get; }

        public Uri EndpointUrl { get; }

        public ConnectionPolicy ConnectionPolicy { get; set; }

        public ConsistencyLevel? ConsistencyLevel { get; set; } = null;

        public IndexingPolicy IndexingPolicy { get; set; } = CosmosConstants.DefaultIndexingPolicy;

        public int DefaultCollectionThroughput { get; set; } =  CosmosConstants.MinimumCosmosThroughput;

        public bool ScaleCollectionRUsAutomatically { get; set; }

        public int MaximumUpscaleRequestUnits { get; set; } = CosmosConstants.DefaultMaximumUpscaleThroughput;

        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        public bool InfiniteRetries { get; set; } = true;

        public string CollectionPrefix { get; set; } = string.Empty;

        public CosmosStoreSettings(string databaseName,
            string endpointUrl,
            string authKey,
            Action<CosmosStoreSettings> settings) : this(databaseName, new Uri(endpointUrl), authKey, settings)
        {
        }

        public CosmosStoreSettings(string databaseName,
            Uri endpointUrl,
            string authKey,
            Action<CosmosStoreSettings> settings)
        {
            DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            EndpointUrl = endpointUrl ?? throw new ArgumentNullException(nameof(endpointUrl));
            AuthKey = authKey ?? throw new ArgumentNullException(nameof(authKey));
            settings?.Invoke(this);
        }
        
        public CosmosStoreSettings(
            string databaseName,
            string endpointUrl,
            string authKey,
            ConnectionPolicy connectionPolicy = null,
            IndexingPolicy indexingPolicy = null,
            int defaultCollectionThroughput = CosmosConstants.MinimumCosmosThroughput,
            bool scaleCollectionRUsAutomatically = false,
            int maximumUpscaleRequestUnits = CosmosConstants.DefaultMaximumUpscaleThroughput)
            : this(databaseName, 
                  new Uri(endpointUrl), 
                  authKey,
                  connectionPolicy,
                  indexingPolicy,
                  defaultCollectionThroughput,
                  scaleCollectionRUsAutomatically,
                  maximumUpscaleRequestUnits)
        {
        }
        
        public CosmosStoreSettings(
            string databaseName, 
            Uri endpointUrl, 
            string authKey,
            ConnectionPolicy connectionPolicy = null,
            IndexingPolicy indexingPolicy = null,
            int defaultCollectionThroughput = CosmosConstants.MinimumCosmosThroughput,
            bool scaleCollectionRUsAutomatically = false,
            int maximumUpscaleRequestUnits = CosmosConstants.DefaultMaximumUpscaleThroughput)
        {
            DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            EndpointUrl = endpointUrl ?? throw new ArgumentNullException(nameof(endpointUrl));
            AuthKey = authKey ?? throw new ArgumentNullException(nameof(authKey));
            ConnectionPolicy = connectionPolicy;
            DefaultCollectionThroughput = defaultCollectionThroughput;
            ScaleCollectionRUsAutomatically = scaleCollectionRUsAutomatically;
            MaximumUpscaleRequestUnits = maximumUpscaleRequestUnits;
            IndexingPolicy = indexingPolicy ?? CosmosConstants.DefaultIndexingPolicy;
        }
    }
}