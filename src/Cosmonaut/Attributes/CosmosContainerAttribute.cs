using System;

namespace Cosmonaut.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CosmosContainerAttribute : Attribute
    {
        public string Name { get; set; }

        public CosmosContainerAttribute(string name)
        {
            Name = name;
        }
    }
}