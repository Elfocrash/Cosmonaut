using Newtonsoft.Json;

namespace Cosmonaut.System.Models
{
    public class Dog : Animal
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}