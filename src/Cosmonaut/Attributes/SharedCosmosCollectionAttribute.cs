using System;

namespace Cosmonaut.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SharedCosmosCollectionAttribute : Attribute
    {
        public string SharedCollectionName { get; }

        public string EntityName { get; }

        public bool UseEntityFullName { get; }

        public SharedCosmosCollectionAttribute(string sharedCollectionName)
        {
            SharedCollectionName = sharedCollectionName;
        }

        public SharedCosmosCollectionAttribute(string sharedCollectionName, string entityName) : this(sharedCollectionName)
        {
            EntityName = entityName;
        }

        public SharedCosmosCollectionAttribute(string sharedCollectionName, bool useEntityFullName) : this(sharedCollectionName)
        {
            UseEntityFullName = useEntityFullName;
        }
    }
}