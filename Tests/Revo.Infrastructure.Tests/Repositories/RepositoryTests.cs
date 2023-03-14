using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Infrastructure.Repositories;
using NSubstitute;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Xunit;

namespace Revo.Infrastructure.Tests.Repositories
{
    public class RepositoryTests
    {
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IUnitOfWorkAccessor unitOfWorkAccessor;
        private readonly IUnitOfWork unitOfWork;
        private readonly IAggregateStore aggregateStore1;
        private readonly IAggregateStore aggregateStore2;
        private readonly IAggregateStoreFactory aggregateStoreFactory1;
        private readonly IAggregateStoreFactory aggregateStoreFactory2;
        private readonly Repository sut;

        private ITransaction uowInnerTransaction = null;

        public RepositoryTests()
        {
            publishEventBuffer = Substitute.For<IPublishEventBuffer>();
            unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.EventBuffer.Returns(publishEventBuffer);
            unitOfWork.When(x => x.AddInnerTransaction(Arg.Any<ITransaction>())).Do(ci => uowInnerTransaction = ci.ArgAt<ITransaction>(0));
            unitOfWorkAccessor = Substitute.For<IUnitOfWorkAccessor>();
            unitOfWorkAccessor.UnitOfWork.Returns(unitOfWork);

            aggregateStore1 = Substitute.For<IAggregateStore>();
            aggregateStore1.CanHandleAggregateType(typeof(MyEntity1)).Returns(true);
            aggregateStoreFactory1 = Substitute.For<IAggregateStoreFactory>();
            aggregateStoreFactory1.CreateAggregateStore(unitOfWork).Returns(aggregateStore1);
            aggregateStoreFactory1.CanHandleAggregateType(typeof(MyEntity1)).Returns(true);

            aggregateStore2 = Substitute.For<IAggregateStore>();
            aggregateStoreFactory2 = Substitute.For<IAggregateStoreFactory>();
            aggregateStoreFactory2.CreateAggregateStore(unitOfWork).Returns(aggregateStore2);

            sut = Substitute.ForPartsOf<Repository>(new[] { aggregateStoreFactory1, aggregateStoreFactory2 }, unitOfWorkAccessor);
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
        public async Task GetManyAsync_FindsCorrectEntities()
        {
            var entity1 = new MyEntity1();
            var entity2 = new MyEntity1();
            aggregateStore1.GetManyAsync<MyEntity1>(
                    Arg.Is<Guid[]>(x => x.Length == 2 && x.Contains(entity1.Id) && x.Contains(entity2.Id)))
                .Returns(new[] {entity1, entity2});

            var result = await sut.GetManyAsync<MyEntity1>(entity1.Id, entity2.Id);
            result.Should().BeEquivalentTo(new[] { entity1, entity2 });
        }
        
        [Fact]
        public async Task Constructor_AddsSaveTransactionToUoW()
        {
            unitOfWork.Received(1).AddInnerTransaction(Arg.Any<ITransaction>());
            uowInnerTransaction.Should().NotBeNull();

            await uowInnerTransaction.CommitAsync();
            sut.Received(1).SaveChangesAsync();
        }

        public class MyEntity1 : AggregateRoot
        {
            public MyEntity1() : base(Guid.NewGuid())
            {
            }

            public void Do()
            {
                Publish(new EventA() {});
            }
        }

        public class EventA : DomainAggregateEvent
        {
        }
    }
}
