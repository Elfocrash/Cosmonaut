using System;

namespace Cosmonaut.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SharedCosmosCollectionAttribute : Attribute
    {
        public string SharedCollectionName { get; set; }

        public string EntityPrefix { get; set; }

        public SharedCosmosCollectionAttribute(string sharedCollectionName)
        {
            SharedCollectionName = sharedCollectionName;
        }

        public SharedCosmosCollectionAttribute(string sharedCollectionName, string entityPrefix) : this(sharedCollectionName)
        {
            EntityPrefix = entityPrefix;
        }
    }
}