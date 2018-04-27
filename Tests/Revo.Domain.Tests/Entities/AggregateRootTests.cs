using System;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Xunit;

namespace Revo.Domain.Tests.Entities
{
    public class AggregateRootTests
    {


        [Fact]
        public void MarkDeleted_ChangesFlag()
        {
            TestAggregate sut = new TestAggregate(Guid.NewGuid());
            Assert.False(sut.IsDeleted);
            sut.Delete();
            Assert.True(sut.IsDeleted);
        }

        [Fact]
        public void Publish_ThrowsIfDeleted()
        {
            TestAggregate sut = new TestAggregate(Guid.NewGuid());
            sut.Delete();

            Assert.Throws<InvalidOperationException>(() =>
            {
                sut.Do();
            });
        }

        public class TestAggregate : AggregateRoot
        {
            public TestAggregate(Guid id) : base(id)
            {
            }

            public void Delete()
            {
                MarkDeleted();
            }

            public void Do()
            {
                Publish(new TestEvent());
            }
        }

        public class TestEvent : DomainAggregateEvent
        {
        }
    }
}
