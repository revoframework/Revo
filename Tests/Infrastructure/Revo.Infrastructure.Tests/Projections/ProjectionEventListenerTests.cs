using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.Projections;
using NSubstitute;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Xunit;

namespace Revo.Infrastructure.Tests.Projections
{
    public class ProjectionEventListenerTests
    {
        private readonly ProjectionEventListener sut;
        private readonly IAsyncEventSequencer<DomainAggregateEvent> sequencer;
        private readonly IEntityEventProjector<MyEntity1> myEntity1Projector;
        private readonly IEntityEventProjector<MyEntity2> myEntity2Projector;
        private readonly IEntityTypeManager entityTypeManager;

        private readonly Guid aggregate1Id = Guid.Parse("F2280E17-8FC0-4F49-B57B-4335AEFC0063");
        private readonly Guid aggregate2Id = Guid.Parse("8D165937-CB74-4052-B9E3-A7E56AA49106");

        public ProjectionEventListenerTests()
        {
            entityTypeManager = Substitute.For<IEntityTypeManager>();
            entityTypeManager.GetClassInfoByClassId(MyEntity1.ClassId).Returns(new DomainClassInfo(MyEntity1.ClassId, null, typeof(MyEntity1)));
            entityTypeManager.GetClassInfoByClassId(MyEntity2.ClassId).Returns(new DomainClassInfo(MyEntity2.ClassId, null, typeof(MyEntity2)));

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

            sut = Substitute.ForPartsOf<ProjectionEventListener>(entityTypeManager);
            sut.EventSequencer.Returns(sequencer);
            sut.GetProjectors(typeof(MyEntity1)).Returns(new[] {myEntity1Projector});
            sut.GetProjectors(typeof(MyEntity2)).Returns(new[] {myEntity2Projector});
        }

        [Fact]
        public async Task OnFinishedEventQueue_FiresProjections()
        {
            var ev1 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate1Id
            }, new Dictionary<string, string>()
            {
                { BasicEventMetadataNames.AggregateClassId, MyEntity1.ClassId.ToString() }
            });

            var ev2 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate2Id
            }, new Dictionary<string, string>()
            {
                { BasicEventMetadataNames.AggregateClassId, MyEntity2.ClassId.ToString() }
            });

            var ev3 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate1Id
            }, new Dictionary<string, string>()
            {
                { BasicEventMetadataNames.AggregateClassId, MyEntity1.ClassId.ToString() }
            });
            
            await sut.HandleAsync(ev1, "MyProjectionEventListener");
            await sut.HandleAsync(ev2, "MyProjectionEventListener");
            await sut.HandleAsync(ev3, "MyProjectionEventListener");

            await sut.OnFinishedEventQueueAsync("MyProjectionEventListener");
            
            myEntity1Projector.Received(1)
                    .ProjectEventsAsync(aggregate1Id, Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(x => x.SequenceEqual(new List<IEventMessage<DomainAggregateEvent>> () { ev1, ev3 })));
            myEntity1Projector.Received(1).CommitChangesAsync();

            myEntity2Projector.Received(1)
                    .ProjectEventsAsync(aggregate2Id, Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(x => x.SequenceEqual(new List<IEventMessage<DomainAggregateEvent>>() { ev2 })));
            myEntity2Projector.Received(1).CommitChangesAsync();
        }
        
        public class MyEntity1 : EventSourcedAggregateRoot
        {
            public static Guid ClassId = Guid.NewGuid();

            public MyEntity1(Guid id) : base(id)
            {
            }
        }

        public class MyEntity2 : EventSourcedAggregateRoot
        {
            public static Guid ClassId = Guid.NewGuid();

            public MyEntity2(Guid id) : base(id)
            {
            }
        }

        public class MyEvent : DomainAggregateEvent
        {
        }
    }
}
