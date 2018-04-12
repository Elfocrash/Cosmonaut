using Newtonsoft.Json;

namespace Cosmonaut
{
    public interface ICosmosEntity
    {
        [JsonProperty(CosmosConstants.CosmosId)]
        string CosmosId { get; set; }
    }
}