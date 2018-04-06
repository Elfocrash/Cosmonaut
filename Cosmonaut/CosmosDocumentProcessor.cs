using System;
using System.Linq;
using System.Reflection;
using Cosmonaut.Attributes;
using Cosmonaut.Exceptions;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace Cosmonaut
{
    internal class CosmosDocumentProcessor<TEntity> where TEntity : class
    {
        internal dynamic GetCosmosDbFriendlyEntity(TEntity entity)
        {
            var validatedEntity = ValidateEntityForCosmosDb(entity);

            //TODO Clean this up. It is a very bad hack
            dynamic mapped = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(validatedEntity));

            SetTheCosmosDbIdBasedOnTheObjectIndex(validatedEntity, mapped);

            RemovePotentialDuplicateIdProperties(mapped);

            return mapped;
        }

        internal string GetDocumentSelfLink(string databaseName, string collectionName, string documentId) =>
            $"dbs/{databaseName}/colls/{collectionName}/docs/{documentId}/";

        internal TEntity ValidateEntityForCosmosDb(TEntity entity)
        {
            var propertyInfos = entity.GetType().GetProperties();

            var containsJsonAttributeIdCount =
                propertyInfos.Count(x => x.GetCustomAttributes<JsonPropertyAttribute>()
                    .Any(attr => attr.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase)))
                + entity.GetType().GetInterfaces().Count(x => x.GetProperties()
                    .Any(prop => prop.GetCustomAttributes<JsonPropertyAttribute>()
                        .Any(attr => attr.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase))));

            if (containsJsonAttributeIdCount > 1)
                throw new MultipleCosmosIdsException(
                    "An entity can only have one cosmos db id. Only one [JsonAttribute(\"id\")] allowed per entity.");

            var idProperty = propertyInfos.FirstOrDefault(x =>
                x.Name.Equals("id", StringComparison.OrdinalIgnoreCase) && x.PropertyType == typeof(string));

            if (idProperty != null && containsJsonAttributeIdCount == 1)
            {
                if (!idProperty.GetCustomAttributes<JsonPropertyAttribute>().Any(x =>
                    x.PropertyName.Equals("id", StringComparison.OrdinalIgnoreCase)))
                    throw new MultipleCosmosIdsException(
                        "An entity can only have one cosmos db id. Either rename the Id property or remove the [JsonAttribute(\"id\")].");
                return entity;
            }

            if (idProperty != null && idProperty.GetValue(entity) == null)
            {
                idProperty.SetValue(entity, Guid.NewGuid().ToString());
            }

            return entity;
        }

        internal void SetTheCosmosDbIdBasedOnTheObjectIndex(TEntity entity, dynamic mapped)
        {
            mapped.id = GetDocumentId(entity);
        }

        internal string GetDocumentId(TEntity entity)
        {
            var propertyInfos = entity.GetType().GetProperties();

            var propertyWithJsonPropertyId =
                propertyInfos.SingleOrDefault(x => x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName == "id");

            if (propertyWithJsonPropertyId != null &&
                !string.IsNullOrEmpty(propertyWithJsonPropertyId.GetValue(entity)?.ToString()))
            {
                return propertyWithJsonPropertyId.GetValue(entity).ToString();
            }

            var propertyNamedId = propertyInfos.SingleOrDefault(x => x.Name.Equals("id", StringComparison.OrdinalIgnoreCase));

            if (propertyNamedId != null)
            {
                if (!string.IsNullOrEmpty(propertyNamedId.GetValue(entity)?.ToString()))
                {
                    return propertyNamedId.GetValue(entity).ToString();
                }

                propertyNamedId.SetValue(entity, Guid.NewGuid().ToString());
                return propertyNamedId.GetValue(entity).ToString();
            }

            var potentialCosmosEntityId = entity.GetType().GetInterface(nameof(ICosmosEntity))?
                .GetProperties()?.SingleOrDefault(x =>
                    x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName == "id");

            if (potentialCosmosEntityId != null &&
                !string.IsNullOrEmpty(potentialCosmosEntityId.GetValue(entity)?.ToString()))
            {
                return potentialCosmosEntityId.GetValue(entity).ToString();
            }

            throw new CosmosEntityWithoutIdException<TEntity>(entity);
        }

        internal static void RemovePotentialDuplicateIdProperties(dynamic mapped)
        {
            if (mapped.Id != null)
            {
                mapped.Remove("Id");
            }

            if (mapped.ID != null)
            {
                mapped.Remove("ID");
            }

            if (mapped.iD != null)
            {
                mapped.Remove("iD");
            }
        }

        internal PartitionKeyDefinition GetPartitionKeyForEntity(Type type)
        {
            var partitionKeyProperties = type.GetProperties()
                .Where(x => x.GetCustomAttribute<CosmosPartitionKeyAttribute>() != null).ToList();

            if (partitionKeyProperties.Count > 1)
                throw new MultiplePartitionKeysException(type);

            if (partitionKeyProperties.Count == 0)
                return null;

            var partitionKeyProperty = partitionKeyProperties.Single();
            var porentialJsonPropertyAttribute = partitionKeyProperty.GetCustomAttribute<JsonPropertyAttribute>();
            if (HasJsonPropertyAttributeId(porentialJsonPropertyAttribute) 
                || partitionKeyProperty.Name.Equals(nameof(ICosmosEntity.CosmosId))
                || partitionKeyProperty.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                return new PartitionKeyDefinition
                {
                    Paths = { "/id" }
                };
            }

            if(porentialJsonPropertyAttribute != null && !string.IsNullOrEmpty(porentialJsonPropertyAttribute.PropertyName))
                return new PartitionKeyDefinition
                {
                    Paths =
                    {
                        $"/{porentialJsonPropertyAttribute.PropertyName}"
                    }
                };

            return new PartitionKeyDefinition
            {
                Paths =
                {
                    $"/{partitionKeyProperty.Name}"
                }
            };
        }

        private static bool HasJsonPropertyAttributeId(JsonPropertyAttribute porentialJsonPropertyAttribute)
        {
            return porentialJsonPropertyAttribute != null && 
                   !string.IsNullOrEmpty(porentialJsonPropertyAttribute.PropertyName)
                   && porentialJsonPropertyAttribute.PropertyName.Equals("id");
        }

        internal PartitionKey GetPartitionKeyValueForEntity(TEntity entity)
        {
            return new PartitionKey(GetPartitionKeyValueAsStringForEntity(entity));
        }

        internal string GetPartitionKeyValueAsStringForEntity(TEntity entity)
        {
            var type = entity.GetType();
            var partitionKeyProperty = type.GetProperties()
                .Where(x => x.GetCustomAttribute<CosmosPartitionKeyAttribute>() != null).ToList();

            if (partitionKeyProperty.Count > 1)
                throw new MultiplePartitionKeysException(type);

            if (partitionKeyProperty.Count == 0)
                return null;

            return partitionKeyProperty.Single().GetValue(entity).ToString();
        }
    }
}