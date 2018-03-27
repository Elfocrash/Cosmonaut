using Newtonsoft.Json;

namespace Cosmonaut.Models
{
    [CosmosCollection("TheBooks")]
    public class Book
    {
        [JsonProperty("id")]
        public string SomeId { get; set; }

        public string Name { get; set; }

        public TestUser Author { get; set; }
    }
}