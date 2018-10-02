using Newtonsoft.Json;

namespace Cosmonaut.Console
{
    public class EntityWithoutPropertyWithIdName
    {
        //This will not work without the attribute.
        [JsonProperty("id")]
        public string SomeIdentifier { get; set; }
        
        public string Data { get; set; }
    }
}