using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cosmonaut.Internal
{
    public class CosmosSerializable
    {
        protected JObject _propertyBag;
        
        protected CosmosSerializable()
        {
            _propertyBag = new JObject();
        }
      
        protected CosmosSerializable(JObject json)
        {
            _propertyBag = new JObject(json);
        }
      
        public override string ToString()
        {
            return _propertyBag.ToString();
        }
          
        internal T GetValue<T>(string propertyName)
        {
            var jtoken = _propertyBag?[propertyName];
            if (jtoken == null) return default;
        
            if (typeof (T).IsEnum && jtoken.Type == JTokenType.String)
                return jtoken.ToObject<T>(JsonSerializer.CreateDefault());
            //if (this.SerializerSettings != null)
            //  return jtoken.ToObject<T>(JsonSerializer.Create(this.SerializerSettings));
            return jtoken.ToObject<T>();
        }

        /// <summary>
        /// Get the value associated with the specified property name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        internal T GetValue<T>(string propertyName, T defaultValue)
        {
            var jtoken = _propertyBag?[propertyName];
            if (jtoken == null) return defaultValue;
        
            if (typeof(T).IsEnum && jtoken.Type == JTokenType.String)
                return jtoken.ToObject<T>(JsonSerializer.CreateDefault());
            //if (this.SerializerSettings != null)
            //  return jtoken.ToObject<T>(JsonSerializer.Create(this.SerializerSettings));
            return jtoken.ToObject<T>();
        }
      
        internal void SetValue(string name, object value)
        {
            if (_propertyBag == null)
                _propertyBag = new JObject();
            if (value != null)
                _propertyBag[name] = JToken.FromObject(value);
            else
                _propertyBag.Remove(name);
        }
    }
}