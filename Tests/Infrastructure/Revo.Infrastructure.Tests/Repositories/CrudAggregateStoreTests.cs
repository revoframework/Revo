using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Events;
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
        private const string TestAggregateClassIdString = "{4057D3AC-2D96-4C25-935D-F72BAC6BA626}";
        private static readonly Guid TestAggregateClassId = Guid.Parse(TestAggregateClassIdString);

        private readonly CrudAggregateStore sut;
        private readonly InMemoryCrudRepository crudRepository;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IEventMessageFactory eventMessageFactory;
        private readonly List<DomainClassInfo> domainClasses;

        public CrudAggregateStoreTests()
        {
            crudRepository = new InMemoryCrudRepository();
            entityTypeManager = Substitute.For<IEntityTypeManager>();
            publishEventBuffer = Substitute.For<IPublishEventBuffer>();

            domainClasses = new List<DomainClassInfo>()
            {
                new DomainClassInfo(TestAggregateClassId, null, typeof(TestAggregate))
            };

            entityTypeManager.GetClassInfoByClassId(Guid.Empty)
                .ReturnsForAnyArgs(ci => domainClasses.Single(x => x.Id == ci.Arg<Guid>()));
            entityTypeManager.TryGetClassInfoByClrType(null)
                .ReturnsForAnyArgs(ci => domainClasses.SingleOrDefault(x => x.ClrType == ci.Arg<Type>()));
            entityTypeManager.GetClassInfoByClrType(null)
                .ReturnsForAnyArgs(ci => domainClasses.Single(x => x.ClrType == ci.Arg<Type>()));

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

            Assert.Equal(TestAggregateClassId, testAggregate.ClassId);
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
            domainClasses.Add(new DomainClassInfo(TestAggregateClassId, null, testAggregate.GetType()));

            sut.Add(testAggregate);
            testAggregate.Do();

            await sut.SaveChangesAsync();

            testAggregate.Received(1).Commit();
        }
        
        [Fact]
        public async Task SaveChanges_CommitsOnlyChangedAggregates()
        {
            TestAggregate testAggregate = Substitute.ForPartsOf<TestAggregate>(new object[] {Guid.NewGuid()});
            domainClasses.Add(new DomainClassInfo(TestAggregateClassId, null, testAggregate.GetType()));

            sut.Add(testAggregate);

            await sut.SaveChangesAsync();

            testAggregate.Received(0).Commit();
        }

        [DomainClassId(TestAggregateClassIdString)]
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
                Publish(new Event1());
            }
        }

        public class Event1 : DomainAggregateEvent
        {
        }
    }
}
