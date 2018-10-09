using Cosmonaut.Attributes;

namespace Cosmonaut.Console
{
    [SharedCosmosCollection("shared", "cars")]
    public class Car : ISharedCosmosEntity
    {
        public string Id { get; set; }

        [CosmosPartitionKey]
        public string Name { get; set; }

        public string CosmosEntityName { get; set; }
    }
}