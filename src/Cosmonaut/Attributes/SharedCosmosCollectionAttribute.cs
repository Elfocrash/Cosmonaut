using System;

namespace Cosmonaut.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SharedCosmosCollectionAttribute : Attribute
    {
        public string SharedCollectionName { get; set; }

        public string EntityName { get; set; }

        public SharedCosmosCollectionAttribute(string sharedCollectionName)
        {
            SharedCollectionName = sharedCollectionName;
        }

        public SharedCosmosCollectionAttribute(string sharedCollectionName, string entityName) : this(sharedCollectionName)
        {
            EntityName = entityName;
        }
    }
}