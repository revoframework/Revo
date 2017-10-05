using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain.Basic;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.Core.ReadModel;
using GTRevo.Infrastructure.Projections;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Projections
{
    public class EntityEventToPocoProjectorTests
    {
        private TestEntityEventToPocoProjector sut;

        public EntityEventToPocoProjectorTests()
        {
        }

        [Fact]
        public async Task ProjectEventsAsync_CallsApplyProjections()
        {
            TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
            aggregate.Commit();
            List<DomainAggregateEvent> events = new List<DomainAggregateEvent>()
            {
                new TestEvent1(),
                new TestEvent2()
            };

            sut = new TestEntityEventToPocoProjector();
            await sut.ProjectEventsAsync(aggregate, events);

            Assert.True(sut.AppliedEvents.SequenceEqual(new List<DomainAggregateEvent>()
            {
                events[0],
                events[1]
            }));
        }

        [Fact]
        public async Task ProjectEventsAsync_CallsOverridesOnlyOnce()
        {
            TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
            aggregate.Commit();
            List<DomainAggregateEvent> events = new List<DomainAggregateEvent>()
            {
                new TestEvent1(),
                new TestEvent2()
            };

            sut = new TestEntityEventToPocoProjectorWithOverrides();
            await sut.ProjectEventsAsync(aggregate, events);

            Assert.True(sut.AppliedEvents.SequenceEqual(new List<DomainAggregateEvent>()
            {
                events[0],
                events[1],
                events[1]
            }));
        }

        [Fact]
        public async Task ProjectEventsAsync_WithSubProjector()
        {
            TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
            aggregate.Commit();
            List<DomainAggregateEvent> events = new List<DomainAggregateEvent>()
            {
                new TestEvent1(),
                new TestEvent3()
            };
            
            sut = new TestEntityEventToPocoProjector();
            var subProjector = Substitute.ForPartsOf<TestSubProjector>(new object[] {sut.AppliedEvents});
            sut.AddSubProjector(subProjector);
            await sut.ProjectEventsAsync(aggregate, events);


            Assert.True(sut.AppliedEvents.SequenceEqual(new List<DomainAggregateEvent>()
            {
                events[0],
                events[1]
            }));

            subProjector.Received(1).Apply((TestEvent3) events[1], aggregate, sut.LastTarget);
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

            public List<DomainAggregateEvent> AppliedEvents { get; private set; } = new List<DomainAggregateEvent>();

            public override async Task CommitChangesAsync()
            {
            }

            protected override async Task<TestReadModel> CreateProjectionTargetAsync(TestAggregate aggregate, IEnumerable<DomainAggregateEvent> events)
            {
                return LastTarget = new TestReadModel();
            }

            protected override Task<TestReadModel> GetProjectionTargetAsync(TestAggregate aggregate)
            {
                throw new NotImplementedException();
            }

            protected virtual void Apply(TestEvent1 ev)
            {
                AppliedEvents.Add(ev);
            }

            protected virtual void Apply(TestEvent2 ev)
            {
                AppliedEvents.Add(ev);
            }
        }

        public class TestEntityEventToPocoProjectorWithOverrides : TestEntityEventToPocoProjector
        {
            protected override void Apply(TestEvent1 ev)
            {
                base.Apply(ev);
            }

            protected new virtual void Apply(TestEvent2 ev)
            {
                base.Apply(ev);
            }
        }

        public class TestSubProjector : ISubEntityEventProjector
        {
            public TestSubProjector(List<DomainAggregateEvent> appliedEvents)
            {
                AppliedEvents = appliedEvents;
            }

            public List<DomainAggregateEvent> AppliedEvents { get; private set; }

            public virtual void Apply(TestEvent3 ev, TestAggregate aggregate, TestReadModel target)
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
