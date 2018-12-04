using System;
using System.Reflection;
using System.Threading.Tasks;
using Cosmonaut.Extensions;
using Cosmonaut.WebJobs.Extensions.Config;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cosmonaut.WebJobs.Extensions.Trigger
{
    internal class CosmosStoreTriggerAttributeBindingProvider<T> : ITriggerBindingProvider where T : class
    {
        private const string CosmosStoreTriggerUserAgentSuffix = "CosmosStoreTriggerFunctions";
        private const string SharedThroughputRequirementException = "Shared throughput collection should have a partition key";
        private const string LeaseCollectionRequiredPartitionKey = "/id";
        private readonly IConfiguration _configuration;
        private readonly INameResolver _nameResolver;
        private readonly CosmosStoreBindingOptions _bindingOptions;
        private readonly ILogger _logger;

        public CosmosStoreTriggerAttributeBindingProvider(IConfiguration configuration, INameResolver nameResolver, CosmosStoreBindingOptions bindingOptions,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _nameResolver = nameResolver;
            _bindingOptions = bindingOptions;
            _logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("CosmosStore"));
        }

        public async Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<CosmosStoreTriggerAttribute>(inherit: false);
            if (attribute == null)
            {
                return null;
            }

            var desiredConnectionMode = _bindingOptions.ConnectionMode;
            var desiredConnectionProtocol = _bindingOptions.Protocol;

            DocumentCollectionInfo documentCollectionLocation;
            DocumentCollectionInfo leaseCollectionLocation;
            var processorOptions = BuildProcessorOptions(attribute);
            processorOptions.StartFromBeginning = attribute.StartFromBeginning;
            if (attribute.MaxItemsPerInvocation > 0)
            {
                processorOptions.MaxItemCount = attribute.MaxItemsPerInvocation;
            }

            IDocumentClient monitoredDocumentClient;
            IDocumentClient leaseDocumentClient;

            try
            {
                var monitoredConnectionString = ResolveConnectionString(attribute.ConnectionStringSetting, nameof(CosmosStoreTriggerAttribute.ConnectionStringSetting));
                if (string.IsNullOrEmpty(monitoredConnectionString))
                {
                    throw new InvalidOperationException("The connection string for the monitored collection is in an invalid format, please use AccountEndpoint=XXXXXX;AccountKey=XXXXXX;.");
                }

                var monitoredCosmosDbConnectionString = new CosmosDBConnectionString(monitoredConnectionString);

                var leasesConnectionString = ResolveAttributeLeasesConnectionString(attribute);
                var leasesConnection = new CosmosDBConnectionString(leasesConnectionString);
                if (leasesConnection.ServiceEndpoint == null)
                {
                    throw new InvalidOperationException("The connection string for the leases collection is in an invalid format, please use AccountEndpoint=XXXXXX;AccountKey=XXXXXX;.");
                }

                var monitoredCollectionName = GetMonitoredCollectionName(attribute);

                documentCollectionLocation = new DocumentCollectionInfo
                {
                    Uri = monitoredCosmosDbConnectionString.ServiceEndpoint,
                    MasterKey = monitoredCosmosDbConnectionString.AuthKey,
                    DatabaseName = ResolveAttributeValue(attribute.DatabaseName),
                    CollectionName = monitoredCollectionName,
                    ConnectionPolicy = {UserAgentSuffix = CosmosStoreTriggerUserAgentSuffix}
                };

                if (desiredConnectionMode.HasValue)
                {
                    documentCollectionLocation.ConnectionPolicy.ConnectionMode = desiredConnectionMode.Value;
                }

                if (desiredConnectionProtocol.HasValue)
                {
                    documentCollectionLocation.ConnectionPolicy.ConnectionProtocol = desiredConnectionProtocol.Value;
                }

                leaseCollectionLocation = new DocumentCollectionInfo
                {
                    Uri = leasesConnection.ServiceEndpoint,
                    MasterKey = leasesConnection.AuthKey,
                    DatabaseName = ResolveAttributeValue(attribute.LeaseDatabaseName),
                    CollectionName = ResolveAttributeValue(attribute.LeaseCollectionName)
                };

                leaseCollectionLocation.ConnectionPolicy.UserAgentSuffix = CosmosStoreTriggerUserAgentSuffix;

                if (desiredConnectionMode.HasValue)
                {
                    leaseCollectionLocation.ConnectionPolicy.ConnectionMode = desiredConnectionMode.Value;
                }

                if (desiredConnectionProtocol.HasValue)
                {
                    leaseCollectionLocation.ConnectionPolicy.ConnectionProtocol = desiredConnectionProtocol.Value;
                }

                var resolvedPreferredLocations = ResolveAttributeValue(attribute.PreferredLocations);
                foreach (var location in CosmosDBUtility.ParsePreferredLocations(resolvedPreferredLocations))
                {
                    documentCollectionLocation.ConnectionPolicy.PreferredLocations.Add(location);
                    leaseCollectionLocation.ConnectionPolicy.PreferredLocations.Add(location);
                }

                if (string.IsNullOrEmpty(documentCollectionLocation.DatabaseName)
                    || string.IsNullOrEmpty(documentCollectionLocation.CollectionName)
                    || string.IsNullOrEmpty(leaseCollectionLocation.DatabaseName)
                    || string.IsNullOrEmpty(leaseCollectionLocation.CollectionName))
                {
                    throw new InvalidOperationException("Cannot establish database and collection values. If you are using environment and configuration values, please ensure these are correctly set.");
                }

                if (documentCollectionLocation.Uri.Equals(leaseCollectionLocation.Uri)
                    && documentCollectionLocation.DatabaseName.Equals(leaseCollectionLocation.DatabaseName)
                    && documentCollectionLocation.CollectionName.Equals(leaseCollectionLocation.CollectionName))
                {
                    throw new InvalidOperationException("The monitored collection cannot be the same as the collection storing the leases.");
                }

                var cosmosStore = new CosmosStore<T>(new CosmosStoreSettings(documentCollectionLocation.DatabaseName, 
                        documentCollectionLocation.Uri, documentCollectionLocation.MasterKey), 
                    attribute.CollectionName);

                monitoredDocumentClient = cosmosStore.CosmonautClient.DocumentClient;

                leaseDocumentClient = new DocumentClient(leasesConnection.ServiceEndpoint, leasesConnection.AuthKey, leaseCollectionLocation.ConnectionPolicy);

                if (attribute.CreateLeaseCollectionIfNotExists)
                {
                    await CreateLeaseCollectionIfNotExistsAsync(leaseDocumentClient, leaseCollectionLocation.DatabaseName, leaseCollectionLocation.CollectionName, attribute.LeasesCollectionThroughput);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Cannot create Collection Information for {attribute.CollectionName} in database {attribute.DatabaseName} with lease {attribute.LeaseCollectionName} in database {attribute.LeaseDatabaseName} : {ex.Message}", ex);
            }

            return new CosmosStoreTriggerBinding<T>(parameter, documentCollectionLocation, leaseCollectionLocation, monitoredDocumentClient, leaseDocumentClient, processorOptions, _logger);
        }

        private string GetMonitoredCollectionName(CosmosStoreTriggerAttribute attribute)
        {
            if (!string.IsNullOrEmpty(ResolveAttributeValue(attribute.CollectionName)))
                return ResolveAttributeValue(attribute.CollectionName);

            var entityType = typeof(T);
            var isSharedCollection = entityType.UsesSharedCollection();

            return isSharedCollection ? entityType.GetSharedCollectionName() : entityType.GetCollectionName();
        }

        internal static TimeSpan ResolveTimeSpanFromMilliseconds(string nameOfProperty, TimeSpan baseTimeSpan, int? attributeValue)
        {
            if (!attributeValue.HasValue || attributeValue.Value == 0)
            {
                return baseTimeSpan;
            }

            if (attributeValue.Value < 0)
            {
                throw new InvalidOperationException($"'{nameOfProperty}' must be greater than 0.");
            }

            return TimeSpan.FromMilliseconds(attributeValue.Value);
        }

        private static async Task CreateLeaseCollectionIfNotExistsAsync(IDocumentClient leaseDocumentClient, string databaseName, string collectionName, int throughput)
        {
            try
            {
                await CosmosDBUtility.CreateDatabaseAndCollectionIfNotExistAsync(leaseDocumentClient, databaseName, collectionName, null, throughput);
            }
            catch (DocumentClientException ex) when (ex.Message.Contains(SharedThroughputRequirementException))
            {
                await CosmosDBUtility.CreateDatabaseAndCollectionIfNotExistAsync(leaseDocumentClient, databaseName, collectionName, LeaseCollectionRequiredPartitionKey, throughput);
            }
        }

        private string ResolveAttributeLeasesConnectionString(CosmosStoreTriggerAttribute attribute)
        {
            // If the lease connection string is not set, use the trigger's

            var connectionString = !string.IsNullOrEmpty(attribute.LeaseConnectionStringSetting) ? 
                ResolveConnectionString(attribute.LeaseConnectionStringSetting, nameof(CosmosStoreTriggerAttribute.LeaseConnectionStringSetting)) :
                ResolveConnectionString(attribute.ConnectionStringSetting, nameof(CosmosStoreTriggerAttribute.ConnectionStringSetting));

            if (string.IsNullOrEmpty(connectionString))
            {
                ThrowMissingConnectionStringExceptionForLease();
            }

            return connectionString;
        }

        private void ThrowMissingConnectionStringExceptionForLease()
        {
            var attributeProperty =
                $"{nameof(CosmosStoreTriggerAttribute)}.{nameof(CosmosStoreTriggerAttribute.LeaseConnectionStringSetting)}";

            var optionsProperty = $"{nameof(CosmosStoreBindingOptions)}.{nameof(CosmosStoreBindingOptions.ConnectionString)}";

            var leaseString = "lease ";

            throw new InvalidOperationException(
                $"The CosmosStoreTrigger {leaseString}connection string must be set either via a '{Constants.DefaultConnectionStringName}' configuration connection string, via the {attributeProperty} property or via {optionsProperty}.");
        }

        internal string ResolveConnectionString(string unresolvedConnectionString, string propertyName)
        {
            // First, resolve the string.
            if (!string.IsNullOrEmpty(unresolvedConnectionString))
            {
                var resolvedString = _configuration.GetConnectionStringOrSetting(unresolvedConnectionString);

                if (string.IsNullOrEmpty(resolvedString))
                {
                    throw new InvalidOperationException($"Unable to resolve app setting for property '{nameof(CosmosStoreTriggerAttribute)}.{propertyName}'. Make sure the app setting exists and has a valid value.");
                }

                return resolvedString;
            }

            // If that didn't exist, fall back to options.
            return _bindingOptions.ConnectionString;
        }

        private ChangeFeedProcessorOptions BuildProcessorOptions(CosmosStoreTriggerAttribute attribute)
        {
            var leasesOptions = _bindingOptions.LeaseOptions;
            var entityType = typeof(T);
            var processorOptions = new ChangeFeedProcessorOptions
            {
                LeasePrefix = ResolveAttributeValue(attribute.LeaseCollectionPrefix) ?? (entityType.UsesSharedCollection() ? $"{entityType.GetSharedCollectionName()}_{entityType.GetSharedCollectionEntityName()}_" : $"{entityType.GetCollectionName()}_"),
                FeedPollDelay = ResolveTimeSpanFromMilliseconds(nameof(CosmosStoreTriggerAttribute.FeedPollDelay), leasesOptions.FeedPollDelay, attribute.FeedPollDelay),
                LeaseAcquireInterval = ResolveTimeSpanFromMilliseconds(nameof(CosmosStoreTriggerAttribute.LeaseAcquireInterval), leasesOptions.LeaseAcquireInterval, attribute.LeaseAcquireInterval),
                LeaseExpirationInterval = ResolveTimeSpanFromMilliseconds(nameof(CosmosStoreTriggerAttribute.LeaseExpirationInterval), leasesOptions.LeaseExpirationInterval, attribute.LeaseExpirationInterval),
                LeaseRenewInterval = ResolveTimeSpanFromMilliseconds(nameof(CosmosStoreTriggerAttribute.LeaseRenewInterval), leasesOptions.LeaseRenewInterval, attribute.LeaseRenewInterval),
                CheckpointFrequency = leasesOptions.CheckpointFrequency ?? new CheckpointFrequency()
            };

            if (attribute.CheckpointInterval > 0)
            {
                processorOptions.CheckpointFrequency.TimeInterval = TimeSpan.FromMilliseconds(attribute.CheckpointInterval);
            }

            if (attribute.CheckpointDocumentCount > 0)
            {
                processorOptions.CheckpointFrequency.ProcessedDocumentCount = attribute.CheckpointDocumentCount;
            }

            return processorOptions;
        }

        private string ResolveAttributeValue(string attributeValue)
        {
            return _nameResolver.ResolveWholeString(attributeValue) ?? attributeValue;
        }
    }
}