using Newtonsoft.Json;

namespace Cosmonaut.Models
{
    [CosmosCollection("TheBooks")]
    public class Book : ICosmosEntity
    {
        public string Name { get; set; }

        public TestUser Author { get; set; }

        public string CosmosId { get; set; }
    }
}