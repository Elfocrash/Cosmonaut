using Cosmonaut.Attributes;
using Newtonsoft.Json;

namespace Cosmonaut.Console
{
    [CosmosCollection("bookwith",Throughput = 5000)]
    public class Book
    {
        [CosmosPartitionKey]
        [JsonProperty("namess")]
        public string Name { get; set; }

        public TestUser Author { get; set; }

        public string AnotherRandomProp { get; set; }
        
        public string Id { get; set; }
    }
}