using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.DataAccess.EF6.Model;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Attributes;
using GTRevo.Infrastructure.Core.Domain.Basic;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.EF6.Repositories;
using GTRevo.Testing.DataAccess.EF6;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.EF6.Tests.Repositories
{
    public class EF6AggregateStoreTests
    {
        private const string TestAggregateClassId = "{4057D3AC-2D96-4C25-935D-F72BAC6BA626}";

        private readonly EF6AggregateStore sut;
        private readonly FakeCrudRepository crudRepository;
        private readonly IModelMetadataExplorer modelMetadataExplorer;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IEventQueue eventQueue;

        public EF6AggregateStoreTests()
        {
            crudRepository = new FakeCrudRepository();
            modelMetadataExplorer = Substitute.For<IModelMetadataExplorer>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();
            eventQueue = Substitute.For<IEventQueue>();

            entityTypeManager.GetClassIdByClrType(typeof(TestAggregate)).Returns(Guid.Parse(TestAggregateClassId));

            sut = new EF6AggregateStore(crudRepository, modelMetadataExplorer, entityTypeManager, eventQueue);
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
        public void SaveChanges_PushesEvents()
        {
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            sut.Add(testAggregate);
            testAggregate.Do();
            var event1 = testAggregate.UncommittedEvents.First();
            sut.SaveChanges();

            eventQueue.Received(1).PushEvent(event1);
        }

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
