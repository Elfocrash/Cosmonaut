using Newtonsoft.Json;

namespace Cosmonaut.System.Models
{
    public class Cat
    {
        public string Name { get; set; }

        [JsonProperty("id")]
        public string CatId { get; set; }
    }
}