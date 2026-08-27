using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TrackoApi.Core.Helpers
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AuditIgnoreAttribute:Attribute
    {
    }
    public class BoolConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ToString() == "1" || reader.Value.ToString() == "true" || reader.Value.ToString().ToUpper() == "YES" || reader.Value.ToString().ToUpper() == "Y" || (Boolean.TryParse(reader.Value.ToString(), out var result) && result);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }
    }
}
