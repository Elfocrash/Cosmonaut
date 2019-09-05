using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cosmonaut.Internal
{
    public class UnixDateTimeConverter : DateTimeConverterBase
    {
        private static DateTime _unixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is DateTime))
                throw new ArgumentException("Invalid datetime", nameof (value));
            var totalSeconds = (long) ((DateTime) value - _unixStartTime).TotalSeconds;
            writer.WriteValue(totalSeconds);
        }
        
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Integer)
                throw new Exception("Invalid reader");
            double num;
            try
            {
                num = Convert.ToDouble(reader.Value, (IFormatProvider) CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new Exception("Invalid reader double value");
            }
            return _unixStartTime.AddSeconds(num);
        }
    }
}