using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using NSubstitute;
using Revo.Core.Events;
using Revo.Infrastructure.Events;
using Xunit;

namespace Revo.EasyNetQ.Tests
{
    public class EasyNetQBusTests
    {
        private IBus bus;
        private IEventMessageFactory eventMessageFactory;
        private EasyNetQBus sut;

        public EasyNetQBusTests()
        {
            bus = Substitute.For<IBus>();
            eventMessageFactory = Substitute.For<IEventMessageFactory>();

            sut = new EasyNetQBus(bus, eventMessageFactory);
        }

        [Fact]
        public async Task PublishAsync_Event()
        {
            var event1 = new Event1();

            var message = Substitute.For<IEventMessageDraft<Event1>>();
            message.Event.Returns(event1);
            message.Metadata.Returns(new Dictionary<string, string>());
            eventMessageFactory.CreateMessageAsync(event1).Returns(message);
            
            await sut.PublishAsync(event1);

            bus.Received(1).PublishAsync(Arg.Is<IEventMessage<Event1>>(
                x => x.GetType().IsConstructedGenericType
                     && x.GetType().GetGenericTypeDefinition() == typeof(EventMessage<>)
                     && x.Event == event1
                     && x.Metadata == message.Metadata));
        }

        [Fact]
        public async Task PublishAsync_EventMessage()
        {
            var event1 = new Event1();

            var message = Substitute.For<IEventMessage<Event1>>();
            message.Event.Returns(event1);
            message.Metadata.Returns(new Dictionary<string, string>());

            await sut.PublishAsync(message);

            bus.Received(1).PublishAsync(Arg.Is<IEventMessage<Event1>>(
                x => x.GetType().IsConstructedGenericType
                     && x.GetType().GetGenericTypeDefinition() == typeof(EventMessage<>)
                     && x.Event == event1
                     && x.Metadata == message.Metadata));
        }

        public class Event1 : IEvent
        {
        }
    }
}
