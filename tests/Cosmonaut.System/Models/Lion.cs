using Cosmonaut.Attributes;

namespace Cosmonaut.System.Models
{
    [SharedCosmosCollection("shared")]
    public class Lion : Animal, ISharedCosmosEntity
    {
        public string Id { get; set; }

        public string CosmosEntityName { get; set; }
    }
}