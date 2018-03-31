using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Domain.ReadModel;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Projections;
using Revo.Testing.Infrastructure;
using NSubstitute;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Xunit;

namespace Revo.Infrastructure.Tests.Projections
{
    public class EntityEventToPocoProjectorTests
    {
        private TestEntityEventToPocoProjector sut;
        private TestAggregate aggregate;
        private List<IEventMessageDraft<DomainAggregateEvent>> eventMessages;

        public EntityEventToPocoProjectorTests()
        {
            aggregate = new TestAggregate(Guid.NewGuid());

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

            aggregate.Commit();
        }

        [Fact]
        public async Task ProjectEventsAsync_CallsApplyProjections()
        {
            sut = new TestEntityEventToPocoProjector();
            await sut.ProjectEventsAsync(aggregate.Id, eventMessages);

            Assert.True(sut.AppliedEvents.SequenceEqual(new List<IEventMessage<DomainAggregateEvent>>()
            {
                eventMessages[0],
                eventMessages[1]
            }));
        }

        [Fact]
        public async Task ProjectEventsAsync_CallsOverridesOnlyOnce()
        {
            sut = new TestEntityEventToPocoProjectorWithOverrides();
            await sut.ProjectEventsAsync(aggregate.Id, eventMessages);

            Assert.True(sut.AppliedEvents.SequenceEqual(new List<IEventMessage<DomainAggregateEvent>>()
            {
                eventMessages[0],
                eventMessages[1],
                eventMessages[1]
            }));
        }

        [Fact]
        public async Task ProjectEventsAsync_WithSubProjector()
        {
            sut = new TestEntityEventToPocoProjector();
            var subProjector = Substitute.ForPartsOf<TestSubProjector>(new object[] {sut.AppliedEvents});
            sut.AddSubProjector(subProjector);
            await sut.ProjectEventsAsync(aggregate.Id, eventMessages);
            
            Assert.True(sut.AppliedEvents.SequenceEqual(new List<IEventMessage<DomainAggregateEvent>>()
            {
                eventMessages[0],
                eventMessages[1],
                eventMessages[2]
            }));

            subProjector.Received(1).Apply((IEventMessage<TestEvent3>)eventMessages[2], aggregate.Id, sut.LastTarget);
        }

        public class TestAggregate : EventSourcedAggregateRoot
        {
            public TestAggregate(Guid id) : base(id)
            {
            }
        }

        public class TestReadModel : ReadModelBase
        {
        }

        public class TestEntityEventToPocoProjector : EntityEventToPocoProjector<TestAggregate, TestReadModel>
        {
            public TestEntityEventToPocoProjector()
            {
            }
            
            public TestReadModel LastTarget { get; private set; }

            public new void AddSubProjector(ISubEntityEventProjector projector)
            {
                base.AddSubProjector(projector);
            }

            public List<IEventMessage<DomainAggregateEvent>> AppliedEvents { get; private set; } = new List<IEventMessage<DomainAggregateEvent>>();

            public override async Task CommitChangesAsync()
            {
            }

            protected override async Task<TestReadModel> CreateProjectionTargetAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events)
            {
                return LastTarget = new TestReadModel();
            }

            protected override Task<TestReadModel> GetProjectionTargetAsync(Guid aggregateId)
            {
                throw new NotImplementedException();
            }

            protected virtual void Apply(IEventMessage<TestEvent1> ev)
            {
                AppliedEvents.Add(ev);
            }

            protected virtual void Apply(IEventMessage<TestEvent2> ev)
            {
                AppliedEvents.Add(ev);
            }
        }

        public class TestEntityEventToPocoProjectorWithOverrides : TestEntityEventToPocoProjector
        {
            protected override void Apply(IEventMessage<TestEvent1> ev)
            {
                base.Apply(ev);
            }

            protected new virtual void Apply(IEventMessage<TestEvent2> ev)
            {
                base.Apply(ev);
            }
        }

        public class TestSubProjector : ISubEntityEventProjector
        {
            public TestSubProjector(List<IEventMessage<DomainAggregateEvent>> appliedEvents)
            {
                AppliedEvents = appliedEvents;
            }

            public List<IEventMessage<DomainAggregateEvent>> AppliedEvents { get; private set; }

            public virtual void Apply(IEventMessage<TestEvent3> ev, Guid aggregateId, TestReadModel target)
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
