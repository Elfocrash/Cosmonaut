using System;

namespace Cosmonaut.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CosmosCollectionAttribute : Attribute
    {
        public string Name { get; set; }

        public int Throughput { get; set; } = -1;
        
        public CosmosCollectionAttribute()
        {
        }

        public CosmosCollectionAttribute(string name)
        {
            Name = name;
        }

        public CosmosCollectionAttribute(string name, int throughput) : this(name)
        {
            Throughput = throughput;
        }
    }
}