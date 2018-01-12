using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Repositories;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Repositories
{
    public class RepositoryTests
    {
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IAggregateStore aggregateStore1;
        private readonly IAggregateStore aggregateStore2;
        private readonly Repository sut;

        public RepositoryTests()
        {
            publishEventBuffer = Substitute.For<IPublishEventBuffer>();

            aggregateStore1 = Substitute.For<IAggregateStore>();
            aggregateStore1.CanHandleAggregateType(typeof(MyEntity1)).Returns(true);

            aggregateStore2 = Substitute.For<IAggregateStore>();

            sut = Substitute.ForPartsOf<Repository>(new[] { aggregateStore2, aggregateStore1 }, publishEventBuffer);
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
        public async Task CreateTransactionAndCommitAsync_CommitsRepository()
        {
            using (var tx = sut.CreateTransaction())
            {
                await tx.CommitAsync();
            }
            
            sut.Received(1).SaveChangesAsync();
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
