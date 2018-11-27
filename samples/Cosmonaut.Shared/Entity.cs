using Newtonsoft.Json;

namespace Cosmonaut.Shared
{
    public class Entity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}