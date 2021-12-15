using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Revo.Core.Events;
using Xunit;

namespace Revo.Core.Tests.Events
{
    public class JsonMetadataTests
    {
        private JsonMetadata sut;
        private JObject json;

        public JsonMetadataTests()
        {
            json = new JObject();
            json["key1"] = "value1";
            json["key2"] = "value2";

            sut = new JsonMetadata(json);
        }

        [Fact]
        public void TryGetValue_ExistingKey()
        {
            sut.TryGetValue("key1", out var value).Should().BeTrue();
            value.Should().Be("value1");
        }

        [Fact]
        public void TryGetValue_NonExistentKey()
        {
            sut.TryGetValue("key3", out var _).Should().BeFalse();
        }

        [Fact]
        public void TryGetValue_Null()
        {
            json["key3"] = null;
            sut.TryGetValue("key3", out var value).Should().BeTrue();
            value.Should().Be(null);
        }

        [Fact]
        public void Count()
        {
            sut.Count.Should().Be(2);
        }

        [Fact]
        public void Keys()
        {
            sut.Keys.Should().BeEquivalentTo("key1", "key2");
        }

        [Fact]
        public void Values()
        {
            sut.Values.Should().BeEquivalentTo("value1", "value2");
        }

        [Fact]
        public void GetEnumerator()
        {
            sut.Should().BeEquivalentTo(new[]
            {
                new KeyValuePair<string, string>("key1", "value1"),
                new KeyValuePair<string, string>("key2", "value2")
            });
        }

        [Fact]
        public void ArrayIndexer()
        {
            sut["key1"].Should().Be("value1");
        }
    }
}