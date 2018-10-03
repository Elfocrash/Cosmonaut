using Newtonsoft.Json;

namespace Cosmonaut.NetCore2WebApi.Model
{
    public class Person
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        public string Name { get; set; }
    }
}