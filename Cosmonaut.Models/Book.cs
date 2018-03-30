using Cosmonaut.Attributes;
using Newtonsoft.Json;

namespace Cosmonaut.Models
{
    [CosmosCollection(Throughput = 10000)]
    public class Book
    {
        public string Name { get; set; }

        public TestUser Author { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}