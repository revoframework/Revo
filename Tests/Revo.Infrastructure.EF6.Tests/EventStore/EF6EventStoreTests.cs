using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.EF6.EventStore;
using Revo.Infrastructure.EF6.EventStore.Model;
using Revo.Infrastructure.EventStore;
using NSubstitute;
using Revo.Core.Types;
using Revo.DataAccess.EF6.InMemory;
using Revo.DataAccess.InMemory;
using Revo.Domain.Events;
using Revo.Infrastructure.EF6.Events;
using Revo.Testing.Core;
using Revo.Testing.Infrastructure;
using Xunit;

namespace Revo.Infrastructure.EF6.Tests.EventStore
{
    public class EF6EventStoreTests
    {
        private readonly EF6EventStore sut;
        private readonly EF6InMemoryCrudRepository inMemoryCrudRepository;
        private readonly IEventSerializer eventSerializer;
        private readonly EventStream[] eventStreams;
        private readonly EventStreamRow[] eventStreamRows;
        private readonly IEventStoreRecord[] storeRecords;

        public EF6EventStoreTests()
        {
            inMemoryCrudRepository = new EF6InMemoryCrudRepository();
            eventSerializer = Substitute.For<IEventSerializer>();

            sut = new EF6EventStore(inMemoryCrudRepository, eventSerializer);

            eventStreams = new[]
            {
                new EventStream(Guid.NewGuid()),
                new EventStream(Guid.NewGuid()),
                new EventStream(Guid.NewGuid())
            };
            inMemoryCrudRepository.AttachRange(eventStreams);

            eventSerializer.SerializeEvent(Arg.Any<Event1>())
                .Returns(ci => ("{\"bar\":" + ci.ArgAt<Event1>(0).Foo + "}", new VersionedTypeId("EventName", 5)));
            eventSerializer.DeserializeEvent(Arg.Any<string>(), new VersionedTypeId("EventName", 5))
                .Returns(ci =>
                    new Event1((int) JObject.Parse(ci.ArgAt<string>(0))["bar"]));
            eventSerializer.SerializeEventMetadata(Arg.Is<IReadOnlyDictionary<string, string>>(x => x.Count == 1
                && x["doh"] == "42")).ReturnsForAnyArgs("doh");
            eventSerializer.DeserializeEventMetadata("doh").ReturnsForAnyArgs(new JsonMetadata(JObject.Parse("{\"doh\":42}")));

            FakeClock.Setup();

            eventStreamRows = new[]
            {
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":1}", "EventName", 5, eventStreams[0].Id, 1, Clock.Current.Now, "{\"doh\":42}"),
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":2}", "EventName", 5, eventStreams[0].Id, 2, Clock.Current.Now, "{\"doh\":42}"),
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":3}", "EventName", 5, eventStreams[1].Id, 3, Clock.Current.Now, "{\"doh\":42}"),
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":4}", "EventName", 5, eventStreams[1].Id, 2, Clock.Current.Now, "{\"doh\":42}"),
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":5}", "EventName", 5, eventStreams[1].Id, 4, Clock.Current.Now, "{\"doh\":42}"),
                new EventStreamRow(Guid.NewGuid(), "{\"bar\":6}", "EventName", 5, eventStreams[1].Id, 5, Clock.Current.Now, "{\"doh\":42}")
            };

            storeRecords = eventStreamRows.Select((x, i) =>
                    new FakeEventStoreRecord()
                    {
                        Event = new Event1(i + 1),
                        AdditionalMetadata = new Dictionary<string, string>() {{"doh", "42"}},
                        EventId = eventStreamRows[i].Id,
                        StreamSequenceNumber = eventStreamRows[i].StreamSequenceNumber
                    })
                .ToArray();

            inMemoryCrudRepository.AttachRange(eventStreamRows);
        }

        [Fact]
        public async Task GetEventAsync_GetsBySeqNumber()
        {
            IEventStoreRecord record = await sut.GetEventAsync(eventStreams[0].Id, 1);

            record.ShouldBeEquivalentTo(storeRecords[0]);
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

            records.ShouldBeEquivalentTo(rowsIndices.Select(x => storeRecords[x]));
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
        [InlineData(1, 2, 5, 2, new int[] { 3, 2 })]
        [InlineData(1, 3, 4, null, new int[] { 2, 4 })]
        public async Task GetEventRangeAsync_ReturnsInRange(int eventStreamIndex, long? minSequenceNumber,
            long? maxSequenceNumber, int? maxCount, int[] rowsIndices)
        {
            var records = await sut.GetEventRangeAsync(eventStreams[eventStreamIndex].Id,
                minSequenceNumber, maxSequenceNumber, maxCount);

            records.ShouldBeEquivalentTo(rowsIndices.Select(x => storeRecords[x]));
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
