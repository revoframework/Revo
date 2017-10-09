using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Core.Domain
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
        public void ApplyEvent_ThrowsIfDeleted()
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
                ApplyEvent(new TestEvent());
            }
        }

        public class TestEvent : DomainAggregateEvent
        {
        }
    }
}
