using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Events;
using Revo.Core.Types;
using Revo.DataAccess.InMemory;
using Revo.Domain.Events;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Async.Generic;
using Revo.Infrastructure.EventStores.Generic;
using Xunit;

namespace Revo.Infrastructure.Tests.EventStores.Generic
{
    public class ExternalEventSourceCatchUpTests
    {
        private ExternalEventSourceCatchUp sut;
        private InMemoryCrudRepository crudRepository;
        private IAsyncEventQueueDispatcher asyncEventQueueDispatcher;
        private IEventSerializer eventSerializer;
        private IEventMessage[] dispatchedMessages;

        public ExternalEventSourceCatchUpTests()
        {
            crudRepository = new InMemoryCrudRepository();
            asyncEventQueueDispatcher = Substitute.For<IAsyncEventQueueDispatcher>();
            eventSerializer = Substitute.For<IEventSerializer>();

            eventSerializer.DeserializeEvent("{}", new VersionedTypeId("TestEvent", 1))
                .Returns(new TestEvent());

            asyncEventQueueDispatcher.WhenForAnyArgs(x => x.DispatchToQueuesAsync(null, null, null))
                .Do(ci => dispatchedMessages = ci.Arg<IEnumerable<IEventMessage>>().ToArray());

            sut = new ExternalEventSourceCatchUp(crudRepository, asyncEventQueueDispatcher,
                eventSerializer);
        }

        [Fact]
        public async Task CatchUpAsync_Dispatches()
        {
            var events = new[]
            {
                new ExternalEventRecord(Guid.Parse("C4BFAF3B-2B5D-461B-8284-70EFA32FC025"), "{}", "TestEvent", 1, "{}")
            };
            crudRepository.AttachRange(events);

            await sut.CatchUpAsync();
            asyncEventQueueDispatcher.ReceivedWithAnyArgs(1).DispatchToQueuesAsync(null, null, null);
            dispatchedMessages.Should().HaveCount(1);
            dispatchedMessages[0].Event.Should().BeOfType<TestEvent>();
            dispatchedMessages[0].Metadata.Should().NotBeNullOrEmpty();
            dispatchedMessages[0].Metadata.GetEventId().Should().Be(events[0].Id);
        }
        
        [Fact]
        public async Task CatchUpAsync_IgnoreAlreadyDispatched()
        {
            var events = new[]
            {
                new ExternalEventRecord(Guid.Parse("C4BFAF3B-2B5D-461B-8284-70EFA32FC025"), "{}", "TestEvent", 1, "{}")
            };
            events[0].MarkDispatchedToAsyncQueues();
            crudRepository.AttachRange(events);

            await sut.CatchUpAsync();
            asyncEventQueueDispatcher.DidNotReceiveWithAnyArgs().DispatchToQueuesAsync(null, null, null);
        }

        private class TestEvent : IEvent
        {
        }
    }
}