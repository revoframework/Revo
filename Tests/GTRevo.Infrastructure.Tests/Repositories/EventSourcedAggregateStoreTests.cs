using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain.Basic;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Infrastructure.Repositories;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Repositories
{
    public class EventSourcedAggregateStoreTests
    {
        private readonly EventSourcedAggregateStore sut;
        private readonly IEventSourcedAggregateRepository eventSourcedRepository;

        public EventSourcedAggregateStoreTests()
        {
            eventSourcedRepository = Substitute.For<IEventSourcedAggregateRepository>();
            sut = new EventSourcedAggregateStore(eventSourcedRepository);
        }

        [Fact]
        public void Add_AddsToRepository()
        {
            TestAggregate aggregate1 = new TestAggregate(Guid.NewGuid());
            sut.Add(aggregate1);

            eventSourcedRepository.Received(1).Add<TestAggregate>(aggregate1);
        }

        [Fact]
        public void Get_GetsFromRepository()
        {
            TestAggregate aggregate1 = new TestAggregate(Guid.NewGuid());
            eventSourcedRepository.Get<TestAggregate>(aggregate1.Id).Returns(aggregate1);

            Assert.Equal(aggregate1, sut.Get<TestAggregate>(aggregate1.Id));
        }

        [Fact]
        public async Task GetAsync_GetsFromRepositoryAsync()
        {
            TestAggregate aggregate1 = new TestAggregate(Guid.NewGuid());
            eventSourcedRepository.GetAsync<TestAggregate>(aggregate1.Id).Returns(aggregate1);

            Assert.Equal(aggregate1, await sut.GetAsync<TestAggregate>(aggregate1.Id));
        }

        [Fact]
        public async Task CanHandleAggregateType_TrueForEventSourced()
        {
            Assert.True(sut.CanHandleAggregateType(typeof(TestAggregate)));
        }

        [Fact]
        public async Task CanHandleAggregateType_FalseForOther()
        {
            Assert.False(sut.CanHandleAggregateType(typeof(NonEventAggregate)));
        }

        public class TestAggregate : EventSourcedAggregateRoot
        {
            public TestAggregate(Guid id) : base(id)
            {
            }
        }

        public class NonEventAggregate : BasicAggregateRoot
        {
            public NonEventAggregate()
            { 
            }
        }
    }
}
