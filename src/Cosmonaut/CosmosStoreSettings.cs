using System;
using Cosmonaut.Configuration;
using Cosmonaut.Storage;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Cosmonaut
{
    public class CosmosStoreSettings
    {
        public string DatabaseId { get; }

        internal string AuthKey { get; }

        public Uri EndpointUrl { get; }

        public ConnectionMode ConnectionMode { get; set; }

        public ConsistencyLevel? ConsistencyLevel { get; set; } = null;

        public IndexingPolicy IndexingPolicy { get; set; } = CosmosConstants.DefaultIndexingPolicy;

        public UniqueKeyPolicy UniqueKeyPolicy { get; set; } = CosmosConstants.DefaultUniqueKeyPolicy;

        public int DefaultContainerThroughput { get; set; } =  CosmosConstants.MinimumCosmosThroughput;

        public int? DefaultDatabaseThroughput { get; set; }

        public ThroughputBehaviour OnDatabaseThroughput { get; set; } = ThroughputBehaviour.UseDatabaseThroughput;

        public CosmosSerializer CosmosSerializer { get; set; } = new CosmosJsonNetSerializer();

        public bool InfiniteRetries { get; set; } = true;

        public string ContainerPrefix { get; set; } = string.Empty;

        public bool ProvisionInfrastructureIfMissing { get; set; } = true;

        public CosmosStoreSettings(string databaseId,
            string endpointUrl,
            string authKey,
            Action<CosmosStoreSettings> settings) : this(databaseId, new Uri(endpointUrl), authKey, settings)
        {
        }

        public CosmosStoreSettings(string databaseId,
            Uri endpointUrl,
            string authKey,
            Action<CosmosStoreSettings> settings)
        {
            DatabaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
            EndpointUrl = endpointUrl ?? throw new ArgumentNullException(nameof(endpointUrl));
            AuthKey = authKey ?? throw new ArgumentNullException(nameof(authKey));
            settings?.Invoke(this);
        }

        public CosmosStoreSettings(
            string databaseId,
            string endpointUrl,
            string authKey,
            ConnectionMode connectionMode = ConnectionMode.Direct,
            IndexingPolicy indexingPolicy = null,
            int defaultContainerThroughput = CosmosConstants.MinimumCosmosThroughput)
            : this(databaseId, 
                  new Uri(endpointUrl), 
                  authKey,
                  connectionMode,
                  indexingPolicy,
                  defaultContainerThroughput)
        {
        }
        
        public CosmosStoreSettings(
            string databaseId, 
            Uri endpointUrl, 
            string authKey,
            ConnectionMode connectionMode = ConnectionMode.Direct,
            IndexingPolicy indexingPolicy = null,
            int defaultContainerThroughput = CosmosConstants.MinimumCosmosThroughput)
        {
            DatabaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
            EndpointUrl = endpointUrl ?? throw new ArgumentNullException(nameof(endpointUrl));
            AuthKey = authKey ?? throw new ArgumentNullException(nameof(authKey));
            ConnectionMode = connectionMode;
            DefaultContainerThroughput = defaultContainerThroughput;

            IndexingPolicy = indexingPolicy ?? CosmosConstants.DefaultIndexingPolicy;
            UniqueKeyPolicy = CosmosConstants.DefaultUniqueKeyPolicy;
        }
    }
}