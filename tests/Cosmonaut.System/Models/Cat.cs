using Newtonsoft.Json;

namespace Cosmonaut.System.Models
{
    public class Cat : Animal
    {
        [JsonProperty("id")]
        public string CatId { get; set; }
    }
}