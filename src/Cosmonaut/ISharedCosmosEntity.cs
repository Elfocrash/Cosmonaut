using Newtonsoft.Json;

namespace Cosmonaut
{
    public interface ISharedCosmosEntity
    {
        [JsonProperty(nameof(CosmosEntityName))]
        string CosmosEntityName { get; set; }
    }
}