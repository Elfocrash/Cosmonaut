using System;
using System.Linq;
using System.Reflection;
using Cosmonaut.Attributes;
using Cosmonaut.Exceptions;
using Humanizer;

namespace Cosmonaut.Extensions
{
    internal static class CollectionExtensions
    {
        internal static string GetCollectionName(this Type entityType)
        {
            var collectionNameAttribute = entityType.GetTypeInfo().GetCustomAttribute<CosmosCollectionAttribute>();

            var collectionName = collectionNameAttribute?.Name;

            return !string.IsNullOrEmpty(collectionName) ? collectionName : entityType.Name.ToLower().Pluralize();
        }

        internal static string GetSharedCollectionEntityName(this Type entityType)
        {
            var collectionNameAttribute = entityType.GetTypeInfo().GetCustomAttribute<SharedCosmosCollectionAttribute>();

            var collectionName = collectionNameAttribute.UseEntityFullName ? entityType.FullName : collectionNameAttribute.EntityName;

            return !string.IsNullOrEmpty(collectionName) ? collectionName : entityType.Name.ToLower().Pluralize();
        }

        internal static string GetSharedCollectionName(this Type entityType)
        {
            var collectionNameAttribute = entityType.GetTypeInfo().GetCustomAttribute<SharedCosmosCollectionAttribute>();

            var collectionName = collectionNameAttribute?.SharedCollectionName;

            if (string.IsNullOrEmpty(collectionName))
                throw new SharedCollectionNameMissingException(entityType);

            return collectionName;
        }

        internal static bool UsesSharedCollection(this Type entityType)
        {
            var hasSharedCosmosCollectionAttribute = entityType.GetTypeInfo().GetCustomAttribute<SharedCosmosCollectionAttribute>() != null;
            var implementsSharedCosmosEntity = entityType.GetTypeInfo().GetInterfaces().Contains(typeof(ISharedCosmosEntity));

            if (hasSharedCosmosCollectionAttribute && !implementsSharedCosmosEntity)
                throw new SharedEntityDoesNotImplementExcepction(entityType);

            if (!hasSharedCosmosCollectionAttribute && implementsSharedCosmosEntity)
                throw new SharedEntityDoesNotHaveAttribute(entityType);

            return hasSharedCosmosCollectionAttribute;
        }
    }
}