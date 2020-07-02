using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Events;
using Revo.Infrastructure.Projections;
using Revo.Testing.Infrastructure;
using NSubstitute;
using Revo.Domain.Events;
using Xunit;

namespace Revo.Infrastructure.Tests.Projections
{
    public class EntityEventProjectorTests
    {
        private TestEntityEventProjectorBase sut;
        private List<IEventMessageDraft<DomainAggregateEvent>> eventMessages;
        private readonly Guid aggregateId = Guid.Parse("85BAE182-8894-47BB-BFAE-E386CF706617");

        public EntityEventProjectorTests()
        {
            eventMessages = new DomainAggregateEvent[]
            {
                new TestEvent1(),
                new TestEvent2(),
                new TestEvent3()
            }.Select((x, i) =>
            {
                var message = x.ToMessageDraft();
                message.SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, (i + 1).ToString());
                return message;
            }).ToList();
        }

        [Fact]
        public async Task ProjectEventsAsync_CallsApplyProjections()
        {
            sut = new TestEntityEventProjector();
            await sut.ProjectEventsAsync(aggregateId, eventMessages);

            sut.AppliedEvents.Should().BeEquivalentTo(
                new[]
                {
                    eventMessages[0],
                    eventMessages[1]
                },
                options => options.WithStrictOrdering());
        }

        [Fact]
        public async Task ProjectEventsAsync_CallsInheritedOnlyOnce()
        {
            sut = new TestEntityEventProjector2WithInherited();
            await sut.ProjectEventsAsync(aggregateId, eventMessages);

            sut.AppliedEvents.Should().BeEquivalentTo(
                new[]
                {
                    eventMessages[0],
                    eventMessages[1]
                },
                options => options.WithStrictOrdering());
        }

        [Fact]
        public async Task ProjectEventsAsync_CallsOverridesOnlyOnce()
        {
            sut = new TestEntityEventProjectorWithOverrides();
            await sut.ProjectEventsAsync(aggregateId, eventMessages);

            sut.AppliedEvents.Should().BeEquivalentTo(
                new[]
                {
                    eventMessages[0],
                    eventMessages[1],
                    eventMessages[1]
                },
                options => options.WithStrictOrdering());
        }

        [Fact]
        public async Task ProjectEventsAsync_WithSubProjector()
        {
            sut = new TestEntityEventProjector();
            var subProjector = Substitute.ForPartsOf<TestSubProjector>(new object[] {sut.AppliedEvents});
            sut.AddSubProjector(subProjector);
            await sut.ProjectEventsAsync(aggregateId, eventMessages);

            sut.AppliedEvents.Should().BeEquivalentTo(
                new[]
                {
                    eventMessages[0],
                    eventMessages[1],
                    eventMessages[2]
                },
                options => options.WithStrictOrdering());

            subProjector.Received(1).Apply((IEventMessage<TestEvent3>)eventMessages[2], aggregateId);
        }

        public class TestEntityEventProjectorBase : EntityEventProjector
        {
            public new void AddSubProjector(ISubEntityEventProjector projector)
            {
                base.AddSubProjector(projector);
            }

            public List<IEventMessage<DomainAggregateEvent>> AppliedEvents { get; private set; } = new List<IEventMessage<DomainAggregateEvent>>();

        }

        public class TestEntityEventProjector : TestEntityEventProjectorBase
        {
            public override async Task CommitChangesAsync()
            {
            }

            protected virtual void Apply(IEventMessage<TestEvent1> ev)
            {
                AppliedEvents.Add(ev);
            }

            public virtual void Apply(IEventMessage<TestEvent2> ev, Guid aggregateId)
            {
                AppliedEvents.Add(ev);
            }
        }

        public class TestEntityEventProjector2Base : TestEntityEventProjectorBase
        {
            protected void Apply(IEventMessage<TestEvent1> ev)
            {
                AppliedEvents.Add(ev);
            }

            protected async Task Apply(IEventMessage<TestEvent2> ev)
            {
                AppliedEvents.Add(ev);
            }
        }

        public class TestEntityEventProjector2WithInherited : TestEntityEventProjector2Base
        {
        }

        public class TestEntityEventProjectorWithOverrides : TestEntityEventProjector
        {
            protected override void Apply(IEventMessage<TestEvent1> ev)
            {
                base.Apply(ev);
            }

            public new virtual void Apply(IEventMessage<TestEvent2> ev, Guid aggregateId)
            {
                base.Apply(ev, aggregateId);
            }
        }

        public class TestSubProjector : ISubEntityEventProjector
        {
            public TestSubProjector(List<IEventMessage<DomainAggregateEvent>> appliedEvents)
            {
                AppliedEvents = appliedEvents;
            }

            public List<IEventMessage<DomainAggregateEvent>> AppliedEvents { get; private set; }

            public virtual void Apply(IEventMessage<TestEvent3> ev, Guid aggregateId)
            {
                AppliedEvents.Add(ev);
            }
        }

        public class TestEvent1 : DomainAggregateEvent
        {
        }

        public class TestEvent2 : DomainAggregateEvent
        {
        }

        public class TestEvent3 : DomainAggregateEvent
        {
        }
    }
}
