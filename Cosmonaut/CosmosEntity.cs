using Newtonsoft.Json;

namespace Cosmonaut
{
    public class CosmosEntity : ICosmosEntity
    {
        [JsonProperty("id")]
        public string CosmosId { get; set; }
    }
}