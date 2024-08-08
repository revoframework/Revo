using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Revo.Core.ValueObjects
{
    public class SingleValueObjectJsonConverter : JsonConverter<ISingleValueObject>

    {
        public override bool HandleNull => true;
        
        private static readonly ConcurrentDictionary<Type, Type> ConstructorArgumentTypes = new ConcurrentDictionary<Type, Type>();

        public override void Write(Utf8JsonWriter writer, ISingleValueObject value, JsonSerializerOptions options) {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                var valueObject = value.Value;
                JsonSerializer.Serialize(writer, valueObject, valueObject.GetType(), options);
            }
        }

        
        public override ISingleValueObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var parameterType = ConstructorArgumentTypes.GetOrAdd(
                typeToConvert,
                x =>
                {
                    var constructors = x.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance).Where(c => c.GetParameters().Count() == 1).ToArray();
                    Type valueType = x.GetInterfaces().First(t =>
                            t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(ISingleValueObject<>))
                        .GetGenericArguments()[0];

                    var constructorInfo = constructors.FirstOrDefault(c => c.GetParameters()[0].ParameterType == valueType)
                                          ?? constructors.FirstOrDefault()
                                          ?? throw new InvalidOperationException($"Single-value type {x.FullName} must define a public single-parameter constructor");

                    return constructorInfo.GetParameters()[0].ParameterType;
                });

            var value = JsonSerializer.Deserialize(ref reader, parameterType, options);
            return (ISingleValueObject)Activator.CreateInstance(typeToConvert, value);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ISingleValueObject).IsAssignableFrom(objectType)
                && objectType.IsClass && !objectType.IsAbstract && !objectType.IsGenericTypeDefinition;
        }
    }
}
