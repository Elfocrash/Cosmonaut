using Cosmonaut.Attributes;
using Newtonsoft.Json;

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