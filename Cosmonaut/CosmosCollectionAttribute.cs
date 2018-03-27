using System;

namespace Cosmonaut
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