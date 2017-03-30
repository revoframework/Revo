using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Domain;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Domain
{
    public class AggregateEventRouterTests
    {
        private readonly IAggregateRoot aggregate;
        private AggregateEventRouter router;

        public AggregateEventRouterTests()
        {
            aggregate = Substitute.For<IAggregateRoot>();
            aggregate.Id.Returns(Guid.NewGuid());
            aggregate.ClassId.Returns(Guid.NewGuid());

            router = new AggregateEventRouter(aggregate);
        }

        [Fact]
        public void ApplyEvent_PushesEventToUncomittedEvents()
        {
            var ev1 = new Event1();
            var ev2 = new Event2();

            router.ApplyEvent(ev1);
            router.ApplyEvent(ev2);

            Assert.Equal(2, router.UncommitedEvents.Count());
            Assert.Equal(ev1, router.UncommitedEvents.ElementAt(0));
            Assert.Equal(ev2, router.UncommitedEvents.ElementAt(1));
        }

        [Fact]
        public void ApplyEvent_SetsAggregateIdAndClassId()
        {
            router.ApplyEvent(new Event1());
            
            Assert.Equal(aggregate.Id, router.UncommitedEvents.ElementAt(0).AggregateId);
            Assert.Equal(aggregate.ClassId, router.UncommitedEvents.ElementAt(0).AggregateClassId);
        }
        
        [Fact]
        public void CommitEvents_ClearsUncomittedEvents()
        {
            router.ApplyEvent(new Event1());
            router.CommitEvents();

            Assert.Equal(0, router.UncommitedEvents.Count());
        }

        [Fact]
        public void Apply_FiresAllHandlers()
        {
            List<DomainAggregateEvent> events1 = new List<DomainAggregateEvent>();
            List<DomainAggregateEvent> events2 = new List<DomainAggregateEvent>();

            var ev1 = new Event1();

            router.Register<Event1>(ev => events1.Add(ev));
            router.Register<Event1>(ev => events2.Add(ev));
            router.ApplyEvent(ev1);

            Assert.Equal(events1[0], ev1);
            Assert.Equal(events2[0], ev1);
        }

        [Fact]
        public void ReplayEvents_ExecutesHandlers()
        {
            List<DomainAggregateEvent> events = new List<DomainAggregateEvent>();

            var ev1 = new Event1();

            router.Register<Event1>(ev => events.Add(ev));
            router.ApplyEvent(ev1);

            Assert.Equal(events[0], ev1);
        }

        [Fact]
        public void ReplayEvents_DoesntAddUncomittedEvents()
        {
            router.ReplayEvents(new []{ new Event1() });
            Assert.Equal(0, router.UncommitedEvents.Count());
        }

        public class Event1 : DomainAggregateEvent
        {
        }

        public class Event2 : DomainAggregateEvent
        {
        }
    }
}
