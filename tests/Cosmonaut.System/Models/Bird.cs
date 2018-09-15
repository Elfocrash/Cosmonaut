using Cosmonaut.Attributes;

namespace Cosmonaut.System.Models
{
    [SharedCosmosCollection("shared")]
    public class Bird : Animal, ISharedCosmosEntity
    {
        [CosmosPartitionKey]
        public string Id { get; set; }

        public string CosmosEntityName { get; set; }
    }
}