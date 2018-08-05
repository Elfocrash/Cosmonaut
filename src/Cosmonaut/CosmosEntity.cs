using Newtonsoft.Json;

namespace Cosmonaut
{
    public class CosmosEntity
    {
        [JsonProperty("id")]
        public string CosmosId { get; set; }
    }
}