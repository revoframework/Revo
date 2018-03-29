using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Revo.Domain.Sagas.Attributes;
using Xunit;

namespace Revo.Domain.Tests.Sagas
{
    public class SagaConventionConfigurationScannerTests
    {
        [Fact]
        public void GetSagaConfiguration_GetsEventInfos()
        {
            var configurationInfo = SagaConventionConfigurationScanner.GetSagaConfiguration(typeof(Saga1));

            Assert.Equal(1, configurationInfo.Events.Count);
            Assert.True(configurationInfo.Events.TryGetValue(typeof(Event1),
                out var eventInfos));
            Assert.Equal(1, eventInfos.Count);
            var eventInfo = eventInfos.ElementAt(0);

            Saga1 saga = new Saga1(Guid.NewGuid());
            var event1 = new EventMessage<Event1>(new Event1(), new Dictionary<string, string>());
            eventInfo.HandleDelegate(saga, event1);

            Assert.Equal(1, saga.HandledEvents.Count);
            Assert.Equal(event1, saga.HandledEvents[0]);
        }

        [Fact]
        public void GetSagaConfiguration_GetsInherited()
        {
            var configurationInfo = SagaConventionConfigurationScanner.GetSagaConfiguration(typeof(Saga2));

            configurationInfo.Events.Count.Should().Be(2);
            configurationInfo.Events.Keys.Should().Contain(typeof(Event1));
            configurationInfo.Events.Keys.Should().Contain(typeof(Event2));
        }

        [Fact]
        public void GetSagaConfiguration_GetsPublicHandlers()
        {
            var configurationInfo = SagaConventionConfigurationScanner.GetSagaConfiguration(typeof(Saga6PublicHandlers));
            configurationInfo.Events.Count.Should().Be(2);
        }

        [Fact]   
        public void GetSagaConfiguration_GetsAttributeConfiguredEvent()
        {
            var event1 = new EventMessage<Event1>(new Event1() { Foo = 5 }, new Dictionary<string, string>());
            var configurationInfo = SagaConventionConfigurationScanner.GetSagaConfiguration(typeof(Saga3));

            configurationInfo.Events.TryGetValue(typeof(Event1),
                out var eventInfos).Should().BeTrue();
            eventInfos.Count.Should().Be(1);
            var eventInfo = eventInfos.ElementAt(0);

            eventInfo.EventKeyExpression(event1.Event).Should().Be("5");
            eventInfo.SagaKey.Should().Be("foo");
            eventInfo.IsStartingIfSagaNotFound.Should().BeTrue();
        }

        [Fact]   
        public void GetSagaConfiguration_EventKeyExpressionToStringWithoutFormat()
        {
            Guid bar = Guid.Parse("{3A9A28C9-1776-4D17-A3DA-AA54AC618076}");
            var event1 = new EventMessage<Event1>(new Event1() { Foo = 5, Bar = bar }, new Dictionary<string, string>());
            var configurationInfo = SagaConventionConfigurationScanner.GetSagaConfiguration(typeof(Saga4));

            configurationInfo.Events.TryGetValue(typeof(Event1),
                out var eventInfos).Should().BeTrue();
            eventInfos.Count.Should().Be(1);
            var eventInfo = eventInfos.ElementAt(0);
            
            eventInfo.EventKeyExpression(event1.Event).Should().Be(bar.ToString());
        }

        [Fact]   
        public void GetSagaConfiguration_MultipleAttributesPerEvent()
        {
            var configurationInfo = SagaConventionConfigurationScanner.GetSagaConfiguration(typeof(Saga5));

            Assert.Equal(1, configurationInfo.Events.Count);
            Assert.True(configurationInfo.Events.TryGetValue(typeof(Event1),
                out var eventInfos));
            Assert.Equal(2, eventInfos.Count);
            var eventInfo1 = eventInfos.ElementAt(0);
            var eventInfo2 = eventInfos.ElementAt(1);
            
            Assert.Equal("foo", eventInfo1.SagaKey);
            Assert.Equal("bar", eventInfo2.SagaKey);
        }

        public class Saga1 : EventSourcedSaga
        {
            public Saga1(Guid id) : base(id)
            {
            }

            public List<IEventMessage<DomainEvent>> HandledEvents { get; } = new List<IEventMessage<DomainEvent>>();

            [SagaEvent(IsAlwaysStarting = true)]
            private void Handle(IEventMessage<Event1> ev)
            {
                HandledEvents.Add(ev);
            }
        }

        public class Saga2 : Saga1
        {
            public Saga2(Guid id) : base(id)
            {
            }

            [SagaEvent(IsAlwaysStarting = true)]
            private void Handle(IEventMessage<Event2> ev)
            {
                HandledEvents.Add(ev);
            }
        }

        public class Saga3 : EventSourcedSaga
        {
            public Saga3(Guid id) : base(id)
            {
            }

            public List<IEventMessage<DomainEvent>> HandledEvents { get; } = new List<IEventMessage<DomainEvent>>();
            
            [SagaEvent(EventKey = "Foo", SagaKey = "foo", IsStartingIfSagaNotFound = true)]
            private void Handle(IEventMessage<Event1> ev)
            {
                HandledEvents.Add(ev);
            }
        }

        public class Saga4 : EventSourcedSaga
        {
            public Saga4(Guid id) : base(id)
            {
            }

            public List<IEventMessage<DomainEvent>> HandledEvents { get; } = new List<IEventMessage<DomainEvent>>();
            
            [SagaEvent(EventKey = "Bar", SagaKey = "bar")]
            private void Handle(IEventMessage<Event1> ev)
            {
                HandledEvents.Add(ev);
            }
        }

        public class Saga5 : EventSourcedSaga
        {
            public Saga5(Guid id) : base(id)
            {
            }

            public List<IEventMessage<DomainEvent>> HandledEvents { get; } = new List<IEventMessage<DomainEvent>>();

            [SagaEvent(EventKey = "Foo", SagaKey = "foo")]
            [SagaEvent(EventKey = "Bar", SagaKey = "bar")]
            private void Handle(IEventMessage<Event1> ev)
            {
                HandledEvents.Add(ev);
            }
        }
        
        public class Saga6PublicHandlers : Saga1
        {
            public Saga6PublicHandlers(Guid id) : base(id)
            {
            }

            [SagaEvent(IsAlwaysStarting = true)]
            public void Handle(IEventMessage<Event2> ev)
            {
                HandledEvents.Add(ev);
            }
        }

        public class Event1 : DomainEvent
        {
            public int Foo { get; set; }
            public Guid Bar { get; set; }
        }

        public class Event2 : DomainEvent
        {
        }
    }
}
