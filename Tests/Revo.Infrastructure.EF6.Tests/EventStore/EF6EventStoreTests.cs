using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.EF6.EventStore;
using Revo.Infrastructure.EF6.EventStore.Model;
using Revo.Infrastructure.EventStore;
using Revo.Testing.DataAccess;
using NSubstitute;
using Revo.Domain.Events;
using Xunit;

namespace Revo.Infrastructure.EF6.Tests.EventStore
{
    public class EF6EventStoreTests
    {
        private readonly EF6EventStore sut;
        private readonly FakeCrudRepository fakeCrudRepository;
        private readonly IDomainEventTypeCache domainEventTypeCache;
        private readonly EventStream[] eventStreams;
        private readonly EventStreamRow[] eventStreamRows;

        public EF6EventStoreTests()
        {
            fakeCrudRepository = new FakeCrudRepository();
            domainEventTypeCache = Substitute.For<IDomainEventTypeCache>();

            sut = new EF6EventStore(fakeCrudRepository, domainEventTypeCache);

            eventStreams = new[]
            {
                new EventStream(Guid.NewGuid()),
                new EventStream(Guid.NewGuid())
            };
            fakeCrudRepository.AttachRange(eventStreams);

            eventStreamRows = new[]
            {
                new EventStreamRow(domainEventTypeCache, Guid.NewGuid(), new Event1(), eventStreams[0].Id, 1, Clock.Current.Now, new Dictionary<string, string>()),
                new EventStreamRow(domainEventTypeCache, Guid.NewGuid(), new Event1(), eventStreams[0].Id, 2, Clock.Current.Now, new Dictionary<string, string>()),
                new EventStreamRow(domainEventTypeCache, Guid.NewGuid(), new Event1(), eventStreams[1].Id, 3, Clock.Current.Now, new Dictionary<string, string>()),
                new EventStreamRow(domainEventTypeCache, Guid.NewGuid(), new Event1(), eventStreams[1].Id, 2, Clock.Current.Now, new Dictionary<string, string>()),
                new EventStreamRow(domainEventTypeCache, Guid.NewGuid(), new Event1(), eventStreams[1].Id, 4, Clock.Current.Now, new Dictionary<string, string>()),
                new EventStreamRow(domainEventTypeCache, Guid.NewGuid(), new Event1(), eventStreams[1].Id, 5, Clock.Current.Now, new Dictionary<string, string>())
            };

            fakeCrudRepository.AttachRange(eventStreamRows);
        }

        [Fact]
        public async Task GetEventAsync_GetsBySeqNumber()
        {
            IEventStoreRecord record = await sut.GetEventAsync(eventStreams[0].Id, 1);

            record.Should().Be(eventStreamRows[0]);
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

            records.ShouldBeEquivalentTo(rowsIndices.Select(x => eventStreamRows[x]));
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

            records.ShouldBeEquivalentTo(rowsIndices.Select(x => eventStreamRows[x]));
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
        }
    }
}
