using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Revo.Core.Events;
using Revo.Infrastructure.Events.Async;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class AsyncEventSequencerTests
    {
        private MyAsyncEventSequencer sut;

        public AsyncEventSequencerTests()
        {
            sut = new MyAsyncEventSequencer();
        }

        [Fact]
        public void GetEventSequencing_ForType()
        {
            var sequencing = sut.GetEventSequencing(new EventMessage<Event1>(new Event1(), new Dictionary<string, string>())).ToList();
            sequencing.Should().HaveCount(1);
        }

        [Fact]
        public void GetEventSequencing_DefaultForOtherTypes()
        {
            var sequencing = sut.GetEventSequencing(new EventMessage<Event2>(new Event2(), new Dictionary<string, string>())).ToList();
            sequencing.Should().HaveCount(0);
        }

        [Fact]
        public void ShouldAttemptSynchronousDispatch_OnlyForType()
        {
            bool result = sut.ShouldAttemptSynchronousDispatch(new EventMessage<Event1>(new Event1(), new Dictionary<string, string>()));
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldAttemptSynchronousDispatch_DefaultForOtherTypes()
        {
            bool result = sut.ShouldAttemptSynchronousDispatch(new EventMessage<Event2>(new Event2(), new Dictionary<string, string>()));
            result.Should().BeFalse();
        }

        public class MyAsyncEventSequencer : AsyncEventSequencer<Event1>
        {
            protected override IEnumerable<EventSequencing> GetEventSequencing(IEventMessage<Event1> message)
            {
                yield return new EventSequencing() {EventSequenceNumber = 1, SequenceName = "queue1"};
            }

            protected override bool ShouldAttemptSynchronousDispatch(IEventMessage<Event1> message)
            {
                return true;
            }
        }

        public class Event1 : IEvent
        {
        }

        public class Event2 : IEvent
        {
        }
    }
}
