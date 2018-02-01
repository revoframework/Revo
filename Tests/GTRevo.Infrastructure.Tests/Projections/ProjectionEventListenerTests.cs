using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.Events.Async;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Infrastructure.Projections;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Projections
{
    public class ProjectionEventListenerTests
    {
        private readonly ProjectionEventListener sut;
        private readonly IAsyncEventSequencer<DomainAggregateEvent> sequencer;
        private readonly IEntityEventProjector<MyEntity1> myEntity1Projector;
        private readonly IEntityEventProjector<MyEntity2> myEntity2Projector;
        private readonly IEventSourcedAggregateRepository eventSourcedRepository;
        private readonly IEntityTypeManager entityTypeManager;

        private readonly MyEntity1 aggregate1;
        private readonly MyEntity2 aggregate2;

        public ProjectionEventListenerTests()
        {
            aggregate1 = new MyEntity1(Guid.NewGuid());
            aggregate2 = new MyEntity2(Guid.NewGuid());

            eventSourcedRepository = Substitute.For<IEventSourcedAggregateRepository>();
            eventSourcedRepository.GetAsync(aggregate1.Id).Returns(aggregate1);
            eventSourcedRepository.GetAsync(aggregate2.Id).Returns(aggregate2);

            entityTypeManager = Substitute.For<IEntityTypeManager>();
            entityTypeManager.GetClrTypeByClassId(MyEntity1.ClassId).Returns(typeof(MyEntity1));
            entityTypeManager.GetClrTypeByClassId(MyEntity2.ClassId).Returns(typeof(MyEntity2));

            myEntity1Projector = Substitute.For<IEntityEventProjector<MyEntity1>>();
            myEntity1Projector.ProjectedAggregateType.Returns(typeof(MyEntity1));

            myEntity2Projector = Substitute.For<IEntityEventProjector<MyEntity2>>();
            myEntity2Projector.ProjectedAggregateType.Returns(typeof(MyEntity2));

            sequencer = Substitute.For<IAsyncEventSequencer<DomainAggregateEvent>>();
            sequencer.GetEventSequencing(null).ReturnsForAnyArgs(new[]
            {
                new EventSequencing()
                {
                    SequenceName = "MyProjectionEventListener",
                    EventSequenceNumber = 1
                }
            });
            sequencer.ShouldAttemptSynchronousDispatch(null).ReturnsForAnyArgs(true);

            sut = Substitute.ForPartsOf<ProjectionEventListener>(eventSourcedRepository, entityTypeManager);
            sut.EventSequencer.Returns(sequencer);
            sut.GetProjectors(typeof(MyEntity1)).Returns(new[] {myEntity1Projector});
            sut.GetProjectors(typeof(MyEntity2)).Returns(new[] {myEntity2Projector});
        }

        [Fact]
        public async Task OnFinishedEventQueue_FiresProjections()
        {
            var ev1 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate1.Id
            }, new Dictionary<string, string>()
            {
                { BasicEventMetadataNames.AggregateClassId, MyEntity1.ClassId.ToString() }
            });

            var ev2 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate2.Id
            }, new Dictionary<string, string>()
            {
                { BasicEventMetadataNames.AggregateClassId, MyEntity2.ClassId.ToString() }
            });

            var ev3 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate1.Id
            }, new Dictionary<string, string>()
            {
                { BasicEventMetadataNames.AggregateClassId, MyEntity1.ClassId.ToString() }
            });
            
            await sut.HandleAsync(ev1, "MyProjectionEventListener");
            await sut.HandleAsync(ev2, "MyProjectionEventListener");
            await sut.HandleAsync(ev3, "MyProjectionEventListener");

            await sut.OnFinishedEventQueueAsync("MyProjectionEventListener");
            
            myEntity1Projector.Received(1)
                    .ProjectEventsAsync((IEventSourcedAggregateRoot) aggregate1, Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(x => x.SequenceEqual(new List<IEventMessage<DomainAggregateEvent>> () { ev1, ev3 })));
            myEntity1Projector.Received(1).CommitChangesAsync();

            myEntity2Projector.Received(1)
                    .ProjectEventsAsync((IEventSourcedAggregateRoot) aggregate2, Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(x => x.SequenceEqual(new List<IEventMessage<DomainAggregateEvent>>() { ev2 })));
            myEntity2Projector.Received(1).CommitChangesAsync();
        }
        
        public class MyEntity1 : AggregateRoot, IEventSourcedAggregateRoot
        {
            public static Guid ClassId = Guid.NewGuid();

            public MyEntity1(Guid id) : base(id)
            {
            }

            public void LoadState(AggregateState state)
            {
            }
        }

        public class MyEntity2 : AggregateRoot, IEventSourcedAggregateRoot
        {
            public static Guid ClassId = Guid.NewGuid();

            public MyEntity2(Guid id) : base(id)
            {
            }

            public void LoadState(AggregateState state)
            {
            }
        }

        public class MyEvent : DomainAggregateEvent
        {
        }
    }
}
