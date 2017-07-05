using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Repositories;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Repositories
{
    public class RepositoryTests
    {
        private readonly IEventQueue eventQueue;
        private readonly IAggregateStore aggregateStore1;
        private readonly IAggregateStore aggregateStore2;
        private readonly Repository sut;

        public RepositoryTests()
        {
            eventQueue = Substitute.For<IEventQueue>();

            aggregateStore1 = Substitute.For<IAggregateStore>();
            aggregateStore1.CanHandleAggregateType(typeof(MyEntity1)).Returns(true);

            aggregateStore2 = Substitute.For<IAggregateStore>();

            sut = new Repository(new[] { aggregateStore2, aggregateStore1 }, eventQueue);
        }

        [Fact]
        public void Add_AddsToCorrectStore()
        {
            var entity = new MyEntity1();
            sut.Add(entity);

            aggregateStore1.Received(1).Add(entity);
        }

        [Fact]
        public async Task GetAsync_FindsCorrectEntity()
        {
            var entity = new MyEntity1();
            aggregateStore1.GetAsync<MyEntity1>(entity.Id).Returns(entity);

            var foundEntity = await sut.GetAsync<MyEntity1>(entity.Id);
            Assert.Equal(entity, foundEntity);
        }

        [Fact]
        public async Task SaveChangesAsync_PublishesEvents()
        {
            List<IAggregateRoot> aggregateStoreRoots1 = new List<IAggregateRoot>();
            aggregateStore1.WhenForAnyArgs(x => x.Add<MyEntity1>(null))
                .Do(ci => aggregateStoreRoots1.Add(ci.ArgAt<IAggregateRoot>(0)));
            aggregateStore1.GetTrackedAggregates().Returns(aggregateStoreRoots1);

            var entity = new MyEntity1();
            entity.Do();
            var ev1 = entity.UncommitedEvents.First();

            sut.Add(entity);
            await sut.SaveChangesAsync();

            eventQueue.Received(1).PushEvent(ev1);
        }

        [Fact]
        public async Task SaveChangesAsync_CommitsAggregateChanges()
        {
            List<IAggregateRoot> aggregateStoreRoots1 = new List<IAggregateRoot>();
            aggregateStore1.WhenForAnyArgs(x => x.Add<MyEntity1>(null))
                .Do(ci => aggregateStoreRoots1.Add(ci.ArgAt<IAggregateRoot>(0)));
            aggregateStore1.GetTrackedAggregates().Returns(aggregateStoreRoots1);

            var entity = new MyEntity1();
            entity.Do();

            sut.Add(entity);
            await sut.SaveChangesAsync();

            Assert.Equal(1, entity.Version);
            Assert.Empty(entity.UncommitedEvents);
        }

        public class MyEntity1 : AggregateRoot
        {
            public MyEntity1() : base(Guid.NewGuid())
            {
            }

            public void Do()
            {
                ApplyEvent(new EventA() {});
            }
        }

        public class EventA : DomainAggregateEvent
        {
        }
    }
}
