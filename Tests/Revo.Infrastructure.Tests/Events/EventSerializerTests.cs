using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Events;
using Revo.Core.Types;
using Revo.Infrastructure.Events;
using Xunit;

namespace Revo.Infrastructure.Tests.Events
{
    public class EventSerializerTests
    {
        private EventSerializer sut;
        private IVersionedTypeRegistry versionedTypeRegistry;

        public EventSerializerTests()
        {
            versionedTypeRegistry = Substitute.For<IVersionedTypeRegistry>();
            versionedTypeRegistry.GetTypeInfo<IEvent>(typeof(Event1))
                .Returns(new VersionedType(new VersionedTypeId("Event1", 1), typeof(Event1)));
            versionedTypeRegistry.GetTypeInfo<IEvent>(new VersionedTypeId("Event1", 1))
                .Returns(new VersionedType(new VersionedTypeId("Event1", 1), typeof(Event1)));

            versionedTypeRegistry.GetTypeInfo<IEvent>(typeof(Event2))
                .Returns(new VersionedType(new VersionedTypeId("Event2", 1), typeof(Event2)));
            versionedTypeRegistry.GetTypeInfo<IEvent>(new VersionedTypeId("Event2", 1))
                .Returns(new VersionedType(new VersionedTypeId("Event2", 1), typeof(Event2)));

            sut = new EventSerializer(versionedTypeRegistry, x => x);
        }

        [Fact]
        public void DeserializeEvent()
        {
            var result = sut.DeserializeEvent("{\"fooBar\":123}", new VersionedTypeId("Event1", 1));

            result.Should().BeOfType<Event1>();
            ((Event1)result).FooBar.Should().Be(123);
        }

        [Fact]
        public void DeserializeEventMetadata()
        {
            var result = sut.DeserializeEventMetadata("{\"Bar\":\"123\"}");
            result.Should().HaveCount(1);
            result.Should().Contain(new KeyValuePair<string, string>("Bar", "123"));
        }

        [Fact]
        public void SerializeEvent()
        {
            var result = sut.SerializeEvent(new Event1(123));

            var json = JsonNode.Parse(result.EventJson).AsObject();
            json.Should().HaveCount(1);
            json.ContainsKey("fooBar").Should().BeTrue();
            json["fooBar"].GetValue<int>().Should().Be(123);
            result.TypeId.Should().Be(new VersionedTypeId("Event1", 1));
        }

        [Fact]
        public void SerializeEventMetadata()
        {
            var result = sut.SerializeEventMetadata(new Dictionary<string, string>() { { "Bar", "123" } });
            var json = JsonNode.Parse(result).AsObject();
            json.Should().HaveCount(1);
            json.ContainsKey("Bar").Should().BeTrue();
            json["Bar"].GetValue<string>().Should().Be("123");
        }

        [Fact]
        public void SerializeEvent_DictionariesOriginalCase()
        {
            var result = sut.SerializeEvent(new Event2(
                new Dictionary<string, string>()
                {
                    {"Key", "Value"},
                    {"key", "value"}
                }.ToImmutableDictionary()));

            var json = JsonDocument.Parse(result.EventJson).RootElement;
            json.TryGetProperty("pairs", out var pairsProperty);
            pairsProperty.EnumerateObject().Should().Contain(x => x.Name == "Key" && x.Value.ToString() == "Value");
            pairsProperty.EnumerateObject().Should().Contain(x => x.Name == "key" && x.Value.ToString() == "value");
        }

        [Fact]
        public void SerializeEvent_CustomizeEventJsonSerializer()
        {
            sut = new EventSerializer(versionedTypeRegistry,
                x =>
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    });

            var result = sut.SerializeEvent(new Event1(123));

            var json = JsonNode.Parse(result.EventJson).AsObject();
            json.Should().HaveCount(1);
            json.ContainsKey("foo_bar").Should().BeTrue();
            json["foo_bar"].GetValue<int>().Should().Be(123);
        }

        [Fact]
        public void DeserializeEvent_CustomizeEventJsonSerializer()
        {
            sut = new EventSerializer(versionedTypeRegistry,
                x =>
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    });

            var result = sut.DeserializeEvent("{\"foo_bar\":123}", new VersionedTypeId("Event1", 1));

            result.Should().BeOfType<Event1>();
            ((Event1)result).FooBar.Should().Be(123);
        }

        public class Event1 : IEvent
        {
            public Event1(int fooBar)
            {
                FooBar = fooBar;
            }

            public int FooBar { get; }
        }

        public class Event2 : IEvent
        {
            public Event2(ImmutableDictionary<string, string> pairs)
            {
                Pairs = pairs;
            }

            public ImmutableDictionary<string, string> Pairs { get; }
        }
    }
}
