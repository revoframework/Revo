using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Events;
using Revo.Core.Types;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async.Generic;
using Revo.Infrastructure.EventStores.Generic.Model;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async.Generic
{
    public class QueuedAsyncEventMessageFactoryTests
    {
        private IEventSerializer eventSerializer;
        private QueuedAsyncEventMessageFactory sut;
        private TestEvent deserializedEvent = new();
        private Dictionary<string, string> deserializedMetadata = new();

        public QueuedAsyncEventMessageFactoryTests()
        {
            eventSerializer = Substitute.For<IEventSerializer>();
            eventSerializer.DeserializeEvent("{}", Arg.Is<VersionedTypeId>(x => x.Equals(new VersionedTypeId("TestEvent", 1))))
                .Returns(deserializedEvent);
            eventSerializer.DeserializeEventMetadata("{}").Returns(deserializedMetadata);

            sut = new QueuedAsyncEventMessageFactory(eventSerializer);
        }

        [Fact]
        public void CreateEventMessage_EventStreamRow()
        {
            var queuedEvent = new QueuedAsyncEvent(Guid.NewGuid(), "queue1",
                new EventStreamRow(Guid.NewGuid(), "{}", "TestEvent", 1, Guid.NewGuid(), 1,
                    DateTimeOffset.Now, "{}"), 1);
            var result = sut.CreateEventMessage(queuedEvent);

            result.Event.Should().Be(deserializedEvent);
            result.Metadata.Should().NotBeNull();
        }


        [Fact]
        public void CreateEventMessage_ExternalEventRecord()
        {
            var queuedEvent = new QueuedAsyncEvent(Guid.NewGuid(), "queue1",
                new ExternalEventRecord(Guid.NewGuid(), "{}", "TestEvent", 1, "{}"), 1);
            var result = sut.CreateEventMessage(queuedEvent);

            result.Event.Should().Be(deserializedEvent);
            result.Metadata.Should().NotBeNull();
        }

        public class TestEvent : IEvent
        { }
    }
}