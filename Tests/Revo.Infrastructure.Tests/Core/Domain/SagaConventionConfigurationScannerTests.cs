using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Events;
using NSubstitute;
using Revo.Domain.Events;
using Revo.Domain.Sagas;
using Revo.Domain.Sagas.Attributes;
using Xunit;

namespace Revo.Infrastructure.Tests.Core.Domain
{
    public class SagaConventionConfigurationScannerTests
    {
        [Fact]
        public void GetSagaConfiguration_GetsEventInfos()
        {
            var configurationInfo = SagaConventionConfigurationScanner.GetSagaConfiguration(typeof(Saga1));

            Assert.Equal(1, configurationInfo.Events.Count);
            Assert.True(configurationInfo.Events.TryGetValue(typeof(Event1),
                out SagaConventionEventInfo eventInfo));

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

            Assert.Equal(2, configurationInfo.Events.Count);
            Assert.True(configurationInfo.Events.TryGetValue(typeof(Event1),
                out SagaConventionEventInfo eventInfo));

            Saga1 saga = new Saga1(Guid.NewGuid());
            var event1 = new EventMessage<Event1>(new Event1(), new Dictionary<string, string>());
            eventInfo.HandleDelegate(saga, event1);

            Assert.Equal(1, saga.HandledEvents.Count);
            Assert.Equal(event1, saga.HandledEvents[0]);
        }

        [Fact]   
        public void GetSagaConfiguration_GetsAttributeConfiguredEvent()
        {
            var configurationInfo = SagaConventionConfigurationScanner.GetSagaConfiguration(typeof(Saga3));

            Assert.Equal(1, configurationInfo.Events.Count);
            Assert.True(configurationInfo.Events.TryGetValue(typeof(Event1),
                out SagaConventionEventInfo eventInfo));

            Saga3 saga = new Saga3(Guid.NewGuid());
            var event1 = new EventMessage<Event1>(new Event1() { Foo = 5 }, new Dictionary<string, string>());
            eventInfo.HandleDelegate(saga, event1);

            Assert.Equal(1, saga.HandledEvents.Count);
            Assert.Equal(event1, saga.HandledEvents[0]);

            Assert.Equal("5", eventInfo.EventKeyExpression(event1.Event));
            Assert.Equal("foo", eventInfo.SagaKey);
            Assert.True(eventInfo.IsStartingIfSagaNotFound);
        }

        [Fact]   
        public void GetSagaConfiguration_EventKeyExpressionToStringWithoutFormat()
        {
            var configurationInfo = SagaConventionConfigurationScanner.GetSagaConfiguration(typeof(Saga4));

            Assert.Equal(1, configurationInfo.Events.Count);
            Assert.True(configurationInfo.Events.TryGetValue(typeof(Event1),
                out SagaConventionEventInfo eventInfo));

            Saga4 saga = new Saga4(Guid.NewGuid());
            Guid bar = Guid.Parse("{3A9A28C9-1776-4D17-A3DA-AA54AC618076}");
            var event1 = new EventMessage<Event1>(new Event1() { Foo = 5, Bar = bar }, new Dictionary<string, string>());
            eventInfo.HandleDelegate(saga, event1);

            Assert.Equal(1, saga.HandledEvents.Count);
            Assert.Equal(event1, saga.HandledEvents[0]);

            Assert.Equal("3a9a28c9-1776-4d17-a3da-aa54ac618076", eventInfo.EventKeyExpression(event1.Event));
        }

        public class Saga1 : Saga
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

        public class Saga3 : Saga
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

        public class Saga4 : Saga
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

        public class Saga5 : Saga
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
