using System;

namespace Cosmonaut.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SharedCosmosContainerAttribute : Attribute
    {
        public string SharedContainerName { get; }

        public string EntityName { get; }

        public bool UseEntityFullName { get; }

        public SharedCosmosContainerAttribute(string sharedContainerName)
        {
            SharedContainerName = sharedContainerName;
        }

        public SharedCosmosContainerAttribute(string sharedContainerName, string entityName) : this(sharedContainerName)
        {
            EntityName = entityName;
        }

        public SharedCosmosContainerAttribute(string sharedContainerName, bool useEntityFullName) : this(sharedContainerName)
        {
            UseEntityFullName = useEntityFullName;
        }
    }
}