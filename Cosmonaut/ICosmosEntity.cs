using Newtonsoft.Json;

namespace Cosmonaut
{
    public interface ICosmosEntity
    {
        [JsonProperty("id")]
        string CosmosId { get; set; }
    }
}