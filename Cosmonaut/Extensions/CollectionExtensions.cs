using System;
using System.Linq;
using System.Reflection;
using Cosmonaut.Attributes;
using Cosmonaut.Exceptions;
using Humanizer;
using Microsoft.Azure.Documents;

namespace Cosmonaut.Extensions
{
    internal static class CollectionExtensions
    {
        internal static string GetCollectionPartitionKeyName(this DocumentCollection collection)
        {
            return collection?.PartitionKey?.Paths?.FirstOrDefault()?.Trim('/') ?? string.Empty;
        }

        internal static string GetCollectionName(this Type entityType)
        {
            var collectionNameAttribute = entityType.GetCustomAttribute<CosmosCollectionAttribute>();

            var collectionName = collectionNameAttribute?.Name;

            return !string.IsNullOrEmpty(collectionName) ? collectionName : entityType.Name.ToLower().Pluralize();
        }

        internal static string GetSharedCollectionName(this Type entityType)
        {
            var collectionNameAttribute = entityType.GetCustomAttribute<SharedCosmosCollectionAttribute>();

            var collectionName = collectionNameAttribute?.SharedCollectionName;

            if (string.IsNullOrEmpty(collectionName))
                throw new SharedCollectionNameMissingException(entityType);

            return collectionName;
        }

        internal static bool UsesSharedCollection(this Type entityType)
        {
            var collectionNameAttribute = entityType.GetCustomAttribute<SharedCosmosCollectionAttribute>();
            return collectionNameAttribute != null;
        }

        internal static int GetCollectionThroughputForEntity(this Type entityType, 
            bool allowAttributesToConfigureThroughput,
            int collectionThroughput)
        {
            if (!allowAttributesToConfigureThroughput)
            {
                if (collectionThroughput < CosmosConstants.MinimumCosmosThroughput) throw new IllegalCosmosThroughputException();
                return collectionThroughput;
            }

            var collectionAttribute = entityType.GetCustomAttribute<CosmosCollectionAttribute>();
            var throughput = collectionAttribute != null && collectionAttribute.Throughput != -1 ? collectionAttribute.Throughput : collectionThroughput;

            if (collectionThroughput < CosmosConstants.MinimumCosmosThroughput) throw new IllegalCosmosThroughputException();
            return throughput;
        }
    }
}