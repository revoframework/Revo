using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

            string json = JsonConvert.SerializeObject(value);
            JToken jtoken = JToken.Parse(json);

            jtoken.Type.Should().Be(JTokenType.String);
            jtoken.Value<string>().Should().Be("hello");
        }

        [Fact]
        public void JsonDeserializesAsSingleValue()
        {
            string json = "\"hello\"";
            MyValue value = JsonConvert.DeserializeObject<MyValue>(json);
            value.Value.Should().Be("hello");
        }

        public class MyValue : SingleValueObject<MyValue, string>
        {
            public MyValue(string value) : base(value)
            {
            }
        }
    }
}
