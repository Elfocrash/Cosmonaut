using Cosmonaut.Attributes;

namespace Cosmonaut.System.Models
{
    [SharedCosmosCollection("shared")]
    public class Bird : Animal, ISharedCosmosEntity
    {
        public string Id { get; set; }

        public string CosmosEntityName { get; set; }
    }
}