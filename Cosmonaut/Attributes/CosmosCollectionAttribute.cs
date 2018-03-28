using System;

namespace Cosmonaut.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CosmosCollectionAttribute : Attribute
    {
        public string Name { get; set; }

        public CosmosCollectionAttribute(string name)
        {
            Name = name;
        }
    }
}