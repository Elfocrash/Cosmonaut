using Cosmonaut.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cosmonaut.Shared
{
    [SharedCosmosCollection("sharedobjects")]
    public class SharedEntity : ISharedCosmosEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string CosmosEntityName { get; set; }
    }
}