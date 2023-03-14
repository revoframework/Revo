using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Types;
using Revo.DataAccess.Entities;
using Revo.DataAccess.InMemory;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.EventStores.Generic;
using Revo.Infrastructure.EventStores.Generic.Model;
using Revo.Testing.Core;
using Revo.Testing.Infrastructure;
using Xunit;

namespace Revo.Infrastructure.Tests.EventStores.Generic
{
    public class EventStoreTests
    {
        private readonly EventStore sut;
        private readonly InMemoryCrudRepository inMemoryCrudRepository;
        private readonly IEventSerializer eventSerializer;
        private readonly EventStream[] eventStreams;
        private readonly EventStreamRow[] eventStreamRows;
        private readonly IEventStoreRecord[] expectedStoreRecords;

        public EventStoreTests()
        {
            inMemoryCrudRepository = Substitute.ForPartsOf<InMemoryCrudRepository>();
            
            eventStreams = new[]
            {
                new EventStream(Guid.NewGuid()),
                new EventStream(Guid.NewGuid()),
                new EventStream(Guid.NewGuid())
            };
            inMemoryCrudRepository.AttachRange(eventStreams);

            eventSerializer = Substitute.For<IEventSerializer>();

            eventSerializer.SerializeEvent(null)
                .ReturnsForAnyArgs(ci => ("{\"bar\":" + ci.ArgAt<Event1>(0).Foo + "}", new VersionedTypeId("EventName", 5)));
            eventSerializer.DeserializeEvent(Arg.Any<string>(), new VersionedTypeId("EventName", 5))
                .Returns(ci => new Event1((int) JObject.Parse(ci.ArgAt<string>(0))["bar"]));
            eventSerializer.SerializeEventMetadata(null)
                .ReturnsForAnyArgs(ci => JsonConvert.SerializeObject(ci.Arg<IReadOnlyDictionary<string, string>>()
                    .Append(new KeyValuePair<string, string>("fakeSer", "true"))
                    .ToDictionary(x => x.Key, x => x.Value)));
            eventSerializer.DeserializeEventMetadata(null)
                .ReturnsForAnyArgs(ci =>
                {
                    var json = JObject.Parse(ci.Arg<string>());
                    json["fakeDeser"] = "true";
                    return new JsonMetadata(json);
                });

            FakeClock.Setup();

            eventStreamRows = new[]
            {
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":1}", "EventName", 5, eventStreams[0].Id, 1, Clock.Current.UtcNow, "{\"doh\":\"1\"}"),
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":2}", "EventName", 5, eventStreams[0].Id, 2, Clock.Current.UtcNow, "{\"doh\":\"2\"}"),
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":3}", "EventName", 5, eventStreams[1].Id, 3, Clock.Current.UtcNow, "{\"doh\":\"3\"}"),
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":4}", "EventName", 5, eventStreams[1].Id, 2, Clock.Current.UtcNow, "{\"doh\":\"4\"}"),
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":5}", "EventName", 5, eventStreams[1].Id, 4, Clock.Current.UtcNow, "{\"doh\":\"5\"}"),
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":6}", "EventName", 5, eventStreams[1].Id, 5, Clock.Current.UtcNow, "{\"doh\":\"6\"}")
            };

            expectedStoreRecords = eventStreamRows.Select((x, i) =>
                    new FakeEventStoreRecord()
                    {
                        Event = eventSerializer.DeserializeEvent(x.EventJson, new VersionedTypeId(x.EventName, x.EventVersion)),
                        AdditionalMetadata = new Dictionary<string, string>() { { "doh", (i + 1).ToString() }, { "fakeDeser", "true" } },
                        EventId = eventStreamRows[i].Id,
                        StreamSequenceNumber = eventStreamRows[i].StreamSequenceNumber
                    })
                .ToArray();

            inMemoryCrudRepository.AttachRange(eventStreamRows);

            sut = new EventStore(inMemoryCrudRepository, eventSerializer);
        }
        
        [Fact]
        public async Task CommitChangesAsync_SavesRepository()
        {
            await sut.CommitChangesAsync();
            inMemoryCrudRepository.ReceivedWithAnyArgs(1).SaveChangesAsync();
        }

        [Fact]
        public async Task GetEventAsync_GetsBySeqNumber()
        {
            IEventStoreRecord record = await sut.GetEventAsync(eventStreams[0].Id, 1);

            record.Should().BeEquivalentTo(expectedStoreRecords[0]);
        }

        [Fact]
        public async Task GetEventAsync_ThrowsEntityNotFound()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.GetEventAsync(eventStreams[0].Id, 100));
        }

        [Theory]
        [InlineData(0, new int[] { 0, 1 })]
        [InlineData(1, new int[] { 3, 2, 4, 5 })]
        public async Task GetEventsAsync_ReturnsRecords(int eventStreamIndex, int[] rowsIndices)
        {
            var records = await sut.GetEventsAsync(eventStreams[eventStreamIndex].Id);

            records.Should().BeEquivalentTo(rowsIndices.Select(x => expectedStoreRecords[x]));
        }

        [Fact]
        public async Task GetEventsAsync_DoesntThrowAndReturnsEmptyWhenNoEvents()
        {
            var records = await sut.GetEventsAsync(eventStreams[2].Id);
            records.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEventsAsync_ThrowsWhenStreamNotExists()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(()
                => sut.GetEventsAsync(Guid.Parse("6E1CBB11-FB24-41AB-80F2-B79635CD960B")));
        }

        [Theory]
        [InlineData(1, null, null, 2, new int[] { 3, 2 })]
        [InlineData(1, 2L, 5L, 2, new int[] { 3, 2 })]
        [InlineData(1, 3L, 4L, null, new int[] { 2, 4 })]
        public async Task GetEventRangeAsync_ReturnsInRange(int eventStreamIndex, long? minSequenceNumber,
            long? maxSequenceNumber, int? maxCount, int[] rowsIndices)
        {
            var records = await sut.GetEventRangeAsync(eventStreams[eventStreamIndex].Id,
                minSequenceNumber, maxSequenceNumber, maxCount);

            records.Should().BeEquivalentTo(rowsIndices.Select(x => expectedStoreRecords[x]));
        }

        [Fact]
        public async Task GetStreamMetadataAsync_ReturnsDict()
        {
            eventStreams[0].Metadata = new Dictionary<string, string>()
            {
                { "key", "value"}
            };

            var metadata = await sut.GetStreamMetadataAsync(eventStreams[0].Id);

            metadata.Should().BeEquivalentTo(new Dictionary<string, string>()
            {
                {"key", "value"}
            });
        }

        [Fact]
        public async Task GetEventStreamInfo_GetsInfo()
        {
            var result = await sut.GetStreamInfoAsync(eventStreams[1].Id);

            result.Id.Should().Be(eventStreams[1].Id);
            result.EventCount.Should().Be(4);
            result.Version.Should().Be(5);
        }

        [Fact]
        public async Task PushEventsAsync_AddsToRepository()
        {
            await sut.PushEventsAsync(eventStreams[0].Id, new[]
            {
                new UncommitedEventStoreRecord(new Event1(5), new Dictionary<string, string>() { {"doh", "42"}}), 
            });

            var newRows = inMemoryCrudRepository.GetEntities<EventStreamRow>(EntityState.Added).ToList();
            newRows.Should().HaveCount(1);
            newRows[0].EventJson.Should().Be("{\"bar\":5}");
            newRows[0].EventName.Should().Be("EventName");
            newRows[0].EventVersion.Should().Be(5);
            newRows[0].Id.Should().NotBeEmpty();
            newRows[0].StoreDate.Should().Be(Clock.Current.UtcNow);
            newRows[0].IsDispatchedToAsyncQueues.Should().BeFalse();
            newRows[0].StreamSequenceNumber.Should().Be(3);
            newRows[0].StreamId.Should().Be(eventStreams[0].Id);

            JObject jsonMetadata = JObject.Parse(newRows[0].AdditionalMetadataJson);
            jsonMetadata.Should().Contain("doh", "42");
            jsonMetadata.Should().Contain("fakeSer", "true");
        }

        [Fact]
        public async Task PushEventsAsync_ReturnsRecords()
        {
            var uncommittedRecords = new[]
            {
                new UncommitedEventStoreRecord(new Event1(5), new Dictionary<string, string>() {{"doh", "42"}}),
            };

            var records = await sut.PushEventsAsync(eventStreams[0].Id, uncommittedRecords);

            records.Should().HaveCount(1);
            records.ElementAt(0).AdditionalMetadata.Should().Contain(uncommittedRecords[0].Metadata);
            records.ElementAt(0).Event.Should().BeEquivalentTo(uncommittedRecords[0].Event, cfg => cfg.RespectingRuntimeTypes());
            records.ElementAt(0).EventId.Should().NotBeEmpty();
            records.ElementAt(0).StoreDate.Should().Be(Clock.Current.UtcNow);
            records.ElementAt(0).StreamSequenceNumber.Should().Be(3);
        }
        
        [Fact]
        public async Task PushEventsAsync_HasEventSourceNameMetadata()
        {
            await sut.PushEventsAsync(eventStreams[0].Id, new[]
            {
                new UncommitedEventStoreRecord(new Event1(1), new Dictionary<string, string>() { {"doh", "42"}})
            });

            var newRows = inMemoryCrudRepository.GetEntities<EventStreamRow>(EntityState.Added).ToList();
            newRows.Should().HaveCount(1);

            JObject jsonMetadata = JObject.Parse(newRows[0].AdditionalMetadataJson);
            jsonMetadata.Should().ContainKey(BasicEventMetadataNames.EventSourceName);
            jsonMetadata[BasicEventMetadataNames.EventSourceName]?.ToString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task PushEventsAsync_MultipleHaveCorrectStreamSequenceNumbers()
        {
            await sut.PushEventsAsync(eventStreams[0].Id, new[]
            {
                new UncommitedEventStoreRecord(new Event1(1), new Dictionary<string, string>() { {"doh", "42"}}), 
                new UncommitedEventStoreRecord(new Event1(2), new Dictionary<string, string>() { {"doh", "42"}}),
            });

            await sut.PushEventsAsync(eventStreams[0].Id, new[]
            {
                new UncommitedEventStoreRecord(new Event1(3), new Dictionary<string, string>() { {"doh", "42"}})
            });

            var newRows = inMemoryCrudRepository.GetEntities<EventStreamRow>(EntityState.Added).ToList();
            newRows.Should().HaveCount(3);

            newRows.Should().Contain(x => x.EventJson == "{\"bar\":1}" && x.StreamSequenceNumber == 3);
            newRows.Should().Contain(x => x.EventJson == "{\"bar\":2}" && x.StreamSequenceNumber == 4);
            newRows.Should().Contain(x => x.EventJson == "{\"bar\":3}" && x.StreamSequenceNumber == 5);
        }

        [Fact]
        public async Task PushEventsAsync_WithExpectedVersion()
        {
            await sut.PushEventsAsync(eventStreams[0].Id, new[]
            {
                new UncommitedEventStoreRecord(new Event1(1), new Dictionary<string, string>() { {"doh", "42"}})
            });

            var newRows = inMemoryCrudRepository.GetEntities<EventStreamRow>(EntityState.Added).ToList();
            newRows.Should().HaveCount(1);

            newRows.Should().Contain(x => x.StreamSequenceNumber == 3);
        }

        public class Event1 : IEvent
        {
            public Event1(int foo)
            {
                Foo = foo;
            }

            public int Foo { get; private set; }
        }
    }
}
