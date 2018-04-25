using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Events;
using Revo.DataAccess.EF6.Model;
using Revo.DataAccess.InMemory;
using Revo.Domain.Entities;
using Revo.Domain.Entities.Attributes;
using Revo.Domain.Entities.Basic;
using Revo.Domain.Events;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Repositories;
using Xunit;

namespace Revo.Infrastructure.Tests.Repositories
{
    public class CrudAggregateStoreTests
    {
        private const string TestAggregateClassId = "{4057D3AC-2D96-4C25-935D-F72BAC6BA626}";

        private readonly CrudAggregateStore sut;
        private readonly InMemoryCrudRepository crudRepository;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IEventMessageFactory eventMessageFactory;

        public CrudAggregateStoreTests()
        {
            crudRepository = new InMemoryCrudRepository();
            entityTypeManager = Substitute.For<IEntityTypeManager>();
            publishEventBuffer = Substitute.For<IPublishEventBuffer>();

            entityTypeManager.GetClassIdByClrType(typeof(TestAggregate)).Returns(Guid.Parse(TestAggregateClassId));
            
            eventMessageFactory = Substitute.For<IEventMessageFactory>();
            eventMessageFactory.CreateMessageAsync(null).ReturnsForAnyArgs(ci =>
            {
                var @event = ci.ArgAt<IEvent>(0);
                Type messageType = typeof(EventMessageDraft<>).MakeGenericType(@event.GetType());
                IEventMessageDraft messageDraft = (IEventMessageDraft)messageType.GetConstructor(new[] { @event.GetType() }).Invoke(new[] { @event });
                messageDraft.SetMetadata("TestKey", "TestValue");
                return messageDraft;
            }); // TODO something more lightweight?


            sut = new CrudAggregateStore(crudRepository, entityTypeManager, publishEventBuffer, eventMessageFactory);
        }

        [Fact]
        public async Task SaveChanges_InjectsClassIds()
        {
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            sut.Add(testAggregate);

            await sut.SaveChangesAsync();

            Assert.Equal(Guid.Parse(TestAggregateClassId), testAggregate.ClassId);
        }

        [Fact]
        public async Task SaveChanges_PushesEventsForPublishing()
        {
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            sut.Add(testAggregate);
            testAggregate.Do();
            var event1 = testAggregate.UncommittedEvents.First();

            await sut.SaveChangesAsync();

            publishEventBuffer.Received(1).PushEvent(Arg.Is<IEventMessage>(x => x.Event == event1));
        }

        // TODO test pushed event metadata

        [Fact]
        public async Task SaveChanges_CommitsAggregates()
        {
            TestAggregate testAggregate = Substitute.ForPartsOf<TestAggregate>(new object[] { Guid.NewGuid() });
            sut.Add(testAggregate);
            testAggregate.Do();

            await sut.SaveChangesAsync();

            testAggregate.Received(1).Commit();
        }
        
        [Fact]
        public async Task SaveChanges_CommitsOnlyChangedAggregates()
        {
            TestAggregate testAggregate = Substitute.ForPartsOf<TestAggregate>(new object[] {Guid.NewGuid()});
            sut.Add(testAggregate);

            await sut.SaveChangesAsync();

            testAggregate.Received(0).Commit();
        }

        [DomainClassId(TestAggregateClassId)]
        public class TestAggregate : BasicClassAggregateRoot
        {
            public TestAggregate(Guid id) : base(id)
            {
            }

            public TestAggregate()
            {
            }

            public void Do()
            {
                ApplyEvent(new Event1());
            }
        }

        public class Event1 : DomainAggregateEvent
        {
        }
    }
}
