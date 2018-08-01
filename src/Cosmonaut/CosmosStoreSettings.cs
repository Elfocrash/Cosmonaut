using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Cosmonaut
{
    public class CosmosStoreSettings
    {
        public string DatabaseName { get; set; }

        public string AuthKey { get; set; }

        public Uri EndpointUrl { get; set; }

        public ConnectionPolicy ConnectionPolicy { get; set; } = null;

        public ConsistencyLevel? ConsistencyLevel { get; set; } = null;

        public IndexingPolicy IndexingPolicy { get; set; } = null;

        public int DefaultCollectionThroughput { get; set; } =  CosmosConstants.MinimumCosmosThroughput;

        public bool ScaleCollectionRUsAutomatically { get; set; }

        public int MaximumUpscaleRequestUnits { get; set; } = CosmosConstants.DefaultMaximumUpscaleThroughput;

        public CosmosStoreSettings() { }

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
            DatabaseName = databaseName;
            AuthKey = authKey;
            EndpointUrl = endpointUrl;
            ConnectionPolicy = connectionPolicy;
            DefaultCollectionThroughput = defaultCollectionThroughput;
            ScaleCollectionRUsAutomatically = scaleCollectionRUsAutomatically;
            MaximumUpscaleRequestUnits = maximumUpscaleRequestUnits;
            IndexingPolicy = indexingPolicy;
        }
    }
}