using System;
using System.Linq;
using System.Reflection;
using Cosmonaut.Attributes;
using Cosmonaut.Exceptions;
using Cosmonaut.Internal;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace Cosmonaut.Extensions
{
    public static class DocumentEntityExtensions
    {
        internal static PartitionKeyDefinition GetPartitionKeyDefinitionForEntity(this Type type)
        {
            var partitionKeyProperties = InternalTypeCache.Instance.GetPropertiesFromCache(type)
                .Where(x => x.GetCustomAttribute<CosmosPartitionKeyAttribute>() != null).ToList();

            if (partitionKeyProperties.Count > 1)
                throw new MultiplePartitionKeysException(type);

            if (partitionKeyProperties.Count == 0)
                return null;

            var partitionKeyProperty = partitionKeyProperties.Single();
            var porentialJsonPropertyAttribute = partitionKeyProperty.GetCustomAttribute<JsonPropertyAttribute>();
            if (IsCosmosIdThePartitionKey(porentialJsonPropertyAttribute, partitionKeyProperty))
            {
                return DocumentHelpers.GetPartitionKeyDefinition(CosmosConstants.CosmosId);
            }

            if (porentialJsonPropertyAttribute != null &&
                !string.IsNullOrEmpty(porentialJsonPropertyAttribute.PropertyName))
                return DocumentHelpers.GetPartitionKeyDefinition(porentialJsonPropertyAttribute.PropertyName);

            return DocumentHelpers.GetPartitionKeyDefinition(partitionKeyProperty.Name);
        }

        private static bool IsCosmosIdThePartitionKey(JsonPropertyAttribute porentialJsonPropertyAttribute, PropertyInfo partitionKeyProperty)
        {
            return porentialJsonPropertyAttribute.HasJsonPropertyAttributeId()
                   || partitionKeyProperty.Name.Equals(CosmosConstants.CosmosId, StringComparison.OrdinalIgnoreCase);
        }

        internal static PartitionKey GetPartitionKeyValueForEntity<TEntity>(this TEntity entity) where TEntity : class
        {
            var partitionKeyValue = entity.GetPartitionKeyValueAsStringForEntity();
            return !string.IsNullOrEmpty(partitionKeyValue) ? new PartitionKey(partitionKeyValue) : null;
        }

        internal static string GetPartitionKeyValueAsStringForEntity<TEntity>(this TEntity entity) where TEntity : class
        {
            var type = entity.GetType();
            var partitionKeyProperty = InternalTypeCache.Instance.GetPropertiesFromCache(type)
                .Where(x => x.GetCustomAttribute<CosmosPartitionKeyAttribute>() != null)
                .ToList();

            if (partitionKeyProperty.Count > 1)
                throw new MultiplePartitionKeysException(type);

            return partitionKeyProperty.Count == 0 ? null : partitionKeyProperty.Single().GetValue(entity)?.ToString();
        }

        internal static bool HasPartitionKey(this Type type)
        {
            var partitionKeyProperty = InternalTypeCache.Instance.GetPropertiesFromCache(type)
                .Where(x => x.GetCustomAttribute<CosmosPartitionKeyAttribute>() != null).ToList();

            if (partitionKeyProperty.Count > 1)
                throw new MultiplePartitionKeysException(type);

            return partitionKeyProperty.Count != 0;
        }

        internal static void ValidateEntityForCosmosDb<TEntity>(this TEntity entity) where TEntity : class
        {
            var propertyInfos = InternalTypeCache.Instance.GetPropertiesFromCache(entity.GetType());

            var containsJsonAttributeIdCount = GetCountOfJsonPropertiesWithNameIdForObject(entity, propertyInfos);

            if (containsJsonAttributeIdCount > 1)
                throw new MultipleCosmosIdsException(
                    "An entity can only have one cosmos db id. Only one [JsonAttribute(\"id\")] allowed per entity.");

            var idProperty = propertyInfos.FirstOrDefault(x =>
                x.Name.Equals(CosmosConstants.CosmosId, StringComparison.OrdinalIgnoreCase) && x.PropertyType == typeof(string));

            if (idProperty != null && containsJsonAttributeIdCount == 1)
            {
                CheckIfPropertyHasMultpleIdAttributes(idProperty);
            }

            if (idProperty != null && idProperty.GetValue(entity) == null)
            {
                idProperty.SetValue(entity, Guid.NewGuid().ToString());
            }
        }

        private static void CheckIfPropertyHasMultpleIdAttributes(PropertyInfo idProperty)
        {
            if (!idProperty.GetCustomAttributes<JsonPropertyAttribute>().Any(x => !string.IsNullOrEmpty(x?.PropertyName) &&
                x.PropertyName.Equals(CosmosConstants.CosmosId, StringComparison.OrdinalIgnoreCase)))
                throw new MultipleCosmosIdsException(
                    "An entity can only have one cosmos db id. Either rename the Id property or remove the [JsonAttribute(\"id\")].");
        }

        private static int GetCountOfJsonPropertiesWithNameIdForObject<TEntity>(TEntity entity, PropertyInfo[] propertyInfos) where TEntity : class
        {
            return GetCountOfJsonPropertiesWithNameId(propertyInfos) + GetCountOfJsonPropertyWithNameIdInInterfaces(entity);
        }

        private static int GetCountOfJsonPropertyWithNameIdInInterfaces<TEntity>(TEntity entity) where TEntity : class
        {
            return entity.GetType().GetTypeInfo().GetInterfaces().Count(x => InternalTypeCache.Instance.GetPropertiesFromCache(x)
                .Any(prop => prop.GetCustomAttributes<JsonPropertyAttribute>()
                    .Any(attr => !string.IsNullOrEmpty(attr?.PropertyName) && attr.PropertyName.Equals(CosmosConstants.CosmosId))));
        }

        private static int GetCountOfJsonPropertiesWithNameId(PropertyInfo[] propertyInfos)
        {
            return propertyInfos.Count(x => x.GetCustomAttributes<JsonPropertyAttribute>()
                .Any(attr => !string.IsNullOrEmpty(attr?.PropertyName) && attr.PropertyName.Equals(CosmosConstants.CosmosId)));
        }

        internal static bool HasJsonPropertyAttributeId(this JsonPropertyAttribute porentialJsonPropertyAttribute)
        {
            return porentialJsonPropertyAttribute != null &&
                   !string.IsNullOrEmpty(porentialJsonPropertyAttribute.PropertyName)
                   && porentialJsonPropertyAttribute.PropertyName.Equals(CosmosConstants.CosmosId);
        }

        internal static string GetDocumentId<TEntity>(this TEntity entity) where TEntity : class
        {
            var propertyInfos = InternalTypeCache.Instance.GetPropertiesFromCache(entity.GetType());

            var propertyWithJsonPropertyId =
                propertyInfos.SingleOrDefault(x =>
                {
                    var attr = x?.GetCustomAttribute<JsonPropertyAttribute>();
                    return !string.IsNullOrEmpty(attr?.PropertyName) && attr.PropertyName.Equals(CosmosConstants.CosmosId);
                });

            if (propertyWithJsonPropertyId != null)
            {
                if (string.IsNullOrEmpty(propertyWithJsonPropertyId.GetValue(entity)?.ToString()))
                {
                    propertyWithJsonPropertyId.SetValue(entity, Guid.NewGuid().ToString());
                }

                return propertyWithJsonPropertyId.GetValue(entity)?.ToString();
            }

            var propertyNamedId = propertyInfos.SingleOrDefault(x => x.Name.Equals(CosmosConstants.CosmosId, StringComparison.OrdinalIgnoreCase));

            if (propertyNamedId != null)
            {
                return HandlePropertyNamedId(entity, propertyNamedId);
            }
            
            throw new CosmosEntityWithoutIdException<TEntity>(entity);
        }

        private static string HandlePropertyNamedId<TEntity>(TEntity entity, PropertyInfo propertyNamedId) where TEntity : class
        {
            if (!string.IsNullOrEmpty(propertyNamedId.GetValue(entity)?.ToString()))
            {
                return propertyNamedId.GetValue(entity).ToString();
            }

            propertyNamedId.SetValue(entity, Guid.NewGuid().ToString());
            return propertyNamedId.GetValue(entity).ToString();
        }
    }
}