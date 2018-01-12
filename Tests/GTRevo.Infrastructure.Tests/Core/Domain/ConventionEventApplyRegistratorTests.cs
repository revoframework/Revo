using System.Collections.Generic;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Core.Domain
{
    public class ConventionEventApplyRegistratorTests
    {
        [Fact]
        public void RegisterEvents_RegistersAllHandlers()
        {
            var sut = new ConventionEventApplyRegistrator();

            var entity = new MyEntity();
            var aggregate = Substitute.For<IAggregateRoot>();
            var router = new AggregateEventRouter(aggregate);
            
            sut.RegisterEvents(entity, router);
            var ev1 = new Event1();
            router.ApplyEvent(ev1);

            Assert.Equal(ev1, entity.Events[0]);
        }

        public class MyEntity : IComponent
        {
            public List<DomainAggregateEvent> Events = new List<DomainAggregateEvent>();

            private void Apply(Event1 ev)
            {
                Events.Add(ev);
            }
        }

        public class Event1 : DomainAggregateEvent
        { 
        }
    }
}
