using System;
using System.Linq;
using System.Reflection;
using Cosmonaut.Attributes;
using Cosmonaut.Exceptions;
using Humanizer;

namespace Cosmonaut.Extensions
{
    internal static class ContainerExtensions
    {
        internal static string GetContainerName(this Type entityType)
        {
            var containerNameAttribute = entityType.GetTypeInfo().GetCustomAttribute<CosmosContainerAttribute>();

            var containerName = containerNameAttribute?.Name;

            return !string.IsNullOrEmpty(containerName) ? containerName : entityType.Name.ToLower().Pluralize();
        }

        internal static string GetSharedContainerEntityName(this Type entityType)
        {
            var containerNameAttribute = entityType.GetTypeInfo().GetCustomAttribute<SharedCosmosContainerAttribute>();

            var containerName = containerNameAttribute.UseEntityFullName ? entityType.FullName : containerNameAttribute.EntityName;

            return !string.IsNullOrEmpty(containerName) ? containerName : entityType.Name.ToLower().Pluralize();
        }

        internal static string GetSharedContainerName(this Type entityType)
        {
            var containerNameAttribute = entityType.GetTypeInfo().GetCustomAttribute<SharedCosmosContainerAttribute>();

            var containerName = containerNameAttribute?.SharedContainerName;

            if (string.IsNullOrEmpty(containerName))
                throw new SharedContainerNameMissingException(entityType);

            return containerName;
        }

        internal static bool UsesSharedContainer(this Type entityType)
        {
            var hasSharedCosmosContainerAttribute = entityType.GetTypeInfo().GetCustomAttribute<SharedCosmosContainerAttribute>() != null;
            var implementsSharedCosmosEntity = entityType.GetTypeInfo().GetInterfaces().Contains(typeof(ISharedCosmosEntity));

            if (hasSharedCosmosContainerAttribute && !implementsSharedCosmosEntity)
                throw new SharedEntityDoesNotImplementExcepction(entityType);

            if (!hasSharedCosmosContainerAttribute && implementsSharedCosmosEntity)
                throw new SharedEntityDoesNotHaveAttribute(entityType);

            return hasSharedCosmosContainerAttribute;
        }
    }
}