using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Revo.Core.ValueObjects
{
    public class SingleValueObjectJsonConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<Type, Type> ConstructorArgumentTypes = new ConcurrentDictionary<Type, Type>();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var valueObject = value as ISingleValueObject;
            if (valueObject == null)
            {
                writer.WriteNull();
            }
            else
            {
                serializer.Serialize(writer, valueObject.Value);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var parameterType = ConstructorArgumentTypes.GetOrAdd(
                objectType,
                x =>
                {
                    var constructors = x.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance).Where(c => c.GetParameters().Count() == 1).ToArray();
                    Type valueType = objectType.GetInterfaces().First(t =>
                            t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(ISingleValueObject<>))
                        .GetGenericArguments()[0];

                    var constructorInfo = constructors.FirstOrDefault(c => c.GetParameters()[0].ParameterType == valueType)
                                              ?? constructors.FirstOrDefault()
                                              ?? throw new InvalidOperationException($"Single-value type {x.FullName} must define a public single-parameter constructor");

                    return constructorInfo.GetParameters()[0].ParameterType;
                });

            object value = serializer.Deserialize(reader, parameterType);
            return Activator.CreateInstance(objectType, value);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ISingleValueObject).IsAssignableFrom(objectType)
                && objectType.IsClass && !objectType.IsAbstract && !objectType.IsGenericTypeDefinition;
        }
    }
}
