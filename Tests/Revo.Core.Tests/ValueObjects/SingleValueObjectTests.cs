using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Revo.Core.ValueObjects;
using Xunit;

namespace Revo.Core.Tests.ValueObjects
{
    public class SingleValueObjectTests
    {
        [Fact]
        public void JsonSerializesAsSingleValue()
        {
            var value = new MyValue("hello");

            var json = JsonSerializer.Serialize(value);
            var jdoc = JsonDocument.Parse(json);

            jdoc.RootElement.ValueKind.Should().Be(JsonValueKind.String);
            jdoc.RootElement.GetString().Should().Be("hello");
        }

        [Fact]
        public void JsonDeserializesAsSingleValue()
        {
            string json = "\"hello\"";
            var value = JsonSerializer.Deserialize<MyValue>(json);
            value.Value.Should().Be("hello");
        }

        [JsonConverter(typeof(SingleValueObjectJsonConverter))]
        public class MyValue : SingleValueObject<MyValue, string>
        {
            public MyValue(string value) : base(value)
            {
            }
        }
    }
}
