using System.Collections.Generic;
using FluentAssertions;
using Revo.Core.Events;
using Xunit;

namespace Revo.Core.Tests.Events
{
    public class FilteringMetadataTests
    {
        private FilteringMetadata sut;

        public FilteringMetadataTests()
        {
            sut = new FilteringMetadata(
                new Dictionary<string, string>()
                {
                    {"key1", "value1"},
                    {"key2", "value2"},
                    {"key3", "value3"},
                },
                "key1", "key2");
        }

        [Fact]
        public void TryGetValue_RemovedKey()
        {
            sut.TryGetValue("key1", out var _).Should().BeFalse();
        }

        [Fact]
        public void TryGetValue_ExistingKey()
        {
            sut.TryGetValue("key3", out var value).Should().BeTrue();
            value.Should().Be("value3");
        }

        [Fact]
        public void Count()
        {
            sut.Count.Should().Be(1);
        }

        [Fact]
        public void Keys()
        {
            sut.Keys.Should().BeEquivalentTo("key3");
        }

        [Fact]
        public void Values()
        {
            sut.Values.Should().BeEquivalentTo("value3");
        }

        [Fact]
        public void GetEnumerator()
        {
            sut.Should().BeEquivalentTo(
                new[] { new KeyValuePair<string, string>("key3", "value3") });
        }

        [Fact]
        public void ArrayIndexer()
        {
            sut["key3"].Should().Be("value3");
        }
    }
}