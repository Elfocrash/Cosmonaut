using Cosmonaut.Attributes;
using Newtonsoft.Json;

namespace Cosmonaut.Console
{
    [SharedCosmosCollection("shared", "books")]
    public class Book : ISharedCosmosEntity
    {
        [CosmosPartitionKey]
        [JsonProperty("namess")]
        public string Name { get; set; }

        public Car Author { get; set; }

        public string AnotherRandomProp { get; set; }
        
        public string Id { get; set; }

        public string CosmosEntityName { get; set; }
    }
}