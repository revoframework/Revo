using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Domain;
using GTRevo.Infrastructure.Domain.Events;
using GTRevo.Infrastructure.Repositories;
using GTRevo.Infrastructure.Tests.EventSourcing;
using GTRevo.Platform.Events;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Repositories
{
    public class RepositoryTests
    {
        private readonly IEventQueue eventQueue;

        private readonly Repository sut;

        public RepositoryTests()
        {
            eventQueue = Substitute.For<IEventQueue>();
        }

        [Fact]
        public async Task SaveChangesAsync_PublishesEvents()
        {
            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            var ev = new EventSourcedRepositoryTests.SetFooEvent()
            {
                AggregateId = entityId,
                AggregateClassId = entityClassId
            };

            entity.UncommitedEvents = new List<DomainAggregateEvent>()
            {
                ev
            };

            await sut.SaveChangesAsync();

            eventQueue.Received(1).PushEvent(ev);
        }

        public class MyEntity1 : AggregateRoot
        {
            public MyEntity1(Guid id) : base(id)
            {
            }

            protected MyEntity1()
            {
            }
        }
    }
}
