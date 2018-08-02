using Cosmonaut.Attributes;

namespace Cosmonaut.System.Models
{
    [SharedCosmosCollection("shared")]
    public class Lion : ISharedCosmosEntity
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string CosmosEntityName { get; set; }
    }
}