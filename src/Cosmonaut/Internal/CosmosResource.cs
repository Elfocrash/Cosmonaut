using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cosmonaut.Internal
{
    public class CosmosResource : CosmosSerializable
    {
        private static DateTime UnixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public CosmosResource()
        {
        }
        
        public CosmosResource(JObject json) : base(json)
        {
        }
        
        [JsonProperty("id")]
        public string Id
        {
            get => GetValue<string>("id");
            set => SetValue("id", value);
        }
        
        [JsonProperty(PropertyName = "_rid")]
        public virtual string ResourceId
        {
            get => GetValue<string>("_rid");
            set => SetValue("_rid", (object) value);
        }
        
        [JsonProperty(PropertyName = "_self")]
        public string SelfLink
        {
            get => GetValue<string>("_self");
            set => SetValue("_self", (object) value);
        }
        
        [JsonIgnore]
        public string AltLink { get; set; }
        
        [JsonProperty(PropertyName = "_ts")]
        [JsonConverter(typeof (UnixDateTimeConverter))]
        public virtual DateTime Timestamp
        {
            get => UnixStartTime.AddSeconds(GetValue<double>("_ts"));
            internal set => SetValue("_ts", (object) (ulong) (value - UnixStartTime).TotalSeconds);
        }
        
        [JsonProperty(PropertyName = "_etag")]
        public string ETag
        {
            get => GetValue<string>("_etag");
            set => SetValue("_etag", (object) value);
        }
        
        public void SetPropertyValue(string propertyName, object propertyValue)
        {
            SetValue(propertyName, propertyValue);
        }
        
        public T GetPropertyValue<T>(string propertyName)
        {
            return GetValue<T>(propertyName);
        }
    }
}