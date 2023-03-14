using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Events;
using Xunit;

namespace Revo.EasyNetQ.Tests
{
    public class EventTransportTests
    {
        private readonly IEasyNetQBus easyNetQBus;
        private readonly EventTransport<Event2, Event1> sut;

        public EventTransportTests()
        {
            easyNetQBus = Substitute.For<IEasyNetQBus>();
            sut = new EventTransport<Event2, Event1>(easyNetQBus);
        }

        [Fact]
        public async Task HandleAsync()
        {
            var message = (IEventMessage<Event2>) EventMessage.FromEvent(new Event2(), new Dictionary<string, string>());
            await sut.HandleAsync(message, CancellationToken.None);

            easyNetQBus.Received(1).PublishAsync<Event1>(message);
        }

        private class Event1 : IEvent
        {
        }

        private class Event2 : Event1
        {
        }
    }
}
