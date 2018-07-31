using System;
using Newtonsoft.Json;
using Octokit;

namespace MSBLOC.Web.Util
{
    public class OctokitStringEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = value.GetType();
            string v = null;
            if (t.IsGenericType)
            {
                if (t.GetGenericTypeDefinition() == typeof(StringEnum<>))
                {
                    v = t.GetProperty(nameof(StringEnum<DateTime>.StringValue)).GetValue(value)?.ToString();
                }
            }

            writer.WriteValue(v);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(StringEnum<>);
        }

        public override bool CanRead => false;

        public override bool CanWrite => true;
    }
}
