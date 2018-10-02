using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cosmonaut.Console
{
    public class Person
    {
        //It works without the attribute but it is HIGHLY SUGGESTED that you keep it
        [JsonProperty("id")]
        public string Id { get; set; }
        
        public string Name { get; set; }
    }
}