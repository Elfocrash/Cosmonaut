using Newtonsoft.Json;

namespace Cosmonaut.Models
{
    public class TestUser
    {
        
        public string Test { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}