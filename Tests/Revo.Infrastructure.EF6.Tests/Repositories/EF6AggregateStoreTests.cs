using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.EF6.Model;
using Revo.Infrastructure.EF6.Repositories;
using Revo.Infrastructure.Events;
using Revo.Testing.Infrastructure;
using NSubstitute;
using Revo.DataAccess.InMemory;
using Revo.Domain.Entities;
using Revo.Domain.Entities.Attributes;
using Revo.Domain.Entities.Basic;
using Revo.Domain.Events;
using Xunit;

namespace Revo.Infrastructure.EF6.Tests.Repositories
{
    public class EF6AggregateStoreTests
    {
        private const string TestAggregateClassId = "{4057D3AC-2D96-4C25-935D-F72BAC6BA626}";

        private readonly EF6AggregateStore sut;
        private readonly InMemoryCrudRepository crudRepository;
        private readonly IModelMetadataExplorer modelMetadataExplorer;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IEventMessageFactory eventMessageFactory;

        public EF6AggregateStoreTests()
        {
            crudRepository = new InMemoryCrudRepository();
            modelMetadataExplorer = Substitute.For<IModelMetadataExplorer>();
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


            sut = new EF6AggregateStore(crudRepository, modelMetadataExplorer, entityTypeManager, publishEventBuffer, eventMessageFactory);
        }

        [Fact]
        public void SaveChanges_InjectsClassIds()
        {
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            sut.Add(testAggregate);
            sut.SaveChanges();

            Assert.Equal(Guid.Parse(TestAggregateClassId), testAggregate.ClassId);
        }

        [Fact]
        public void SaveChanges_PushesEventsForPublishing()
        {
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            sut.Add(testAggregate);
            testAggregate.Do();
            var event1 = testAggregate.UncommittedEvents.First();
            sut.SaveChanges();

            publishEventBuffer.Received(1).PushEvent(Arg.Is<IEventMessage>(x => x.Event == event1));
        }

        // TODO test pushed event metadata

        [Fact]
        public void SaveChanges_CommitsAggregates()
        {
            TestAggregate testAggregate = Substitute.ForPartsOf<TestAggregate>(new object[] { Guid.NewGuid() });
            sut.Add(testAggregate);
            testAggregate.Do();
            sut.SaveChanges();

            testAggregate.Received(1).Commit();
        }
        
        [Fact]
        public void SaveChanges_CommitsOnlyChangedAggregates()
        {
            TestAggregate testAggregate = Substitute.ForPartsOf<TestAggregate>(new object[] {Guid.NewGuid()});
            sut.Add(testAggregate);
            sut.SaveChanges();

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
