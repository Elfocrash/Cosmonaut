using Cosmonaut.Attributes;
using Newtonsoft.Json;

namespace Cosmonaut.Console
{
    [CosmosCollection(Throughput = 5000)]
    public class Book
    {
        public string Name { get; set; }

        public TestUser Author { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}