using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Domain;
using GTRevo.Infrastructure.Domain.Events;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Infrastructure.Projections;
using GTRevo.Platform.Transactions;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Domain.Projections
{
    public class ProjectionEventListenerTests
    {
        private readonly ProjectionEventListener sut;
        private readonly List<IEntityEventProjector> entityEventProjectors = new List<IEntityEventProjector>();
        private readonly List<ICrudEntityProjector> crudEntityProjectors = new List<ICrudEntityProjector>();
        private readonly IEventSourcedRepository eventSourcedRepository;
        private readonly IEntityTypeManager entityTypeManager;

        private readonly MyEntity1 aggregate1;
        private readonly MyEntity2 aggregate2;

        public ProjectionEventListenerTests()
        {
            aggregate1 = new MyEntity1(Guid.NewGuid());
            aggregate2 = new MyEntity2(Guid.NewGuid());

            eventSourcedRepository = Substitute.For<IEventSourcedRepository>();
            eventSourcedRepository.GetAsync(aggregate1.Id).Returns(aggregate1);
            eventSourcedRepository.GetAsync(aggregate2.Id).Returns(aggregate2);

            entityTypeManager = Substitute.For<IEntityTypeManager>();
            entityTypeManager.GetClrTypeByClassId(MyEntity1.ClassId).Returns(typeof(MyEntity1));
            entityTypeManager.GetClrTypeByClassId(MyEntity2.ClassId).Returns(typeof(MyEntity2));

            var projector1 = Substitute.For<IEntityEventProjector>();
            projector1.ProjectedAggregateType.Returns(typeof(MyEntity1));
            entityEventProjectors.Add(projector1);

            var projector2 = Substitute.For<IEntityEventProjector>();
            projector2.ProjectedAggregateType.Returns(typeof(MyEntity2));
            entityEventProjectors.Add(projector2);

            sut = new ProjectionEventListener(entityEventProjectors.ToArray(), crudEntityProjectors.ToArray(),
                eventSourcedRepository, entityTypeManager);
        }

        [Fact]
        public async Task OnTransactionSucceededAsync_FiresProjections()
        {
            var tx = Substitute.For<ITransaction>();
            sut.OnTransactionBegin(tx);

            var ev1 = new MyEvent()
            {
                AggregateClassId = MyEntity1.ClassId,
                AggregateId = aggregate1.Id
            };

            var ev2 = new MyEvent()
            {
                AggregateClassId = MyEntity2.ClassId,
                AggregateId = aggregate2.Id
            };

            var ev3 = new MyEvent()
            {
                AggregateClassId = MyEntity1.ClassId,
                AggregateId = aggregate1.Id
            };

            await sut.Handle(ev1);
            await sut.Handle(ev2);
            await sut.Handle(ev3);

            await sut.OnTransactionSucceededAsync(tx);

            var events = new List<DomainAggregateEvent>()
            {
                ev1,
                ev2,
                ev3
            };

            entityEventProjectors[0].Received(1)
                    .ProjectEventsAsync(aggregate1, Arg.Is<IEnumerable<DomainAggregateEvent>>(x => x.SequenceEqual(new List<DomainAggregateEvent>() { ev1, ev3 })));
            entityEventProjectors[0].Received(1).CommitChangesAsync();

            entityEventProjectors[1].Received(1)
                    .ProjectEventsAsync(aggregate2, Arg.Is<IEnumerable<DomainAggregateEvent>>(x => x.SequenceEqual(new List<DomainAggregateEvent>() { ev2 })));
            entityEventProjectors[1].Received(1).CommitChangesAsync();
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
