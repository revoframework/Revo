using System;
using Newtonsoft.Json;

namespace Revo.AspNet.IO
{
    public class BracesGuidJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid)
                || objectType == typeof(Guid?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool isNullable = objectType == typeof(Guid?);

            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    if (isNullable)
                    {
                        return null;
                    }
                    else
                    {
                        return Guid.Empty;
                    }
                case JsonToken.String:
                    string str = reader.Value as string;
                    if (string.IsNullOrEmpty(str))
                    {
                        if (isNullable)
                        {
                            return null;
                        }
                        else
                        {
                            return Guid.Empty;
                        }
                    }
                    else
                    {
                        return new Guid(str);
                    }
                default:
                    throw new ArgumentException("Invalid JSON GUID format: " + reader.Value);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteValue("{" + ((Guid)value).ToString() + "}");
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
