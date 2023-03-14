using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Events;
using NSubstitute;
using Xunit;

namespace Revo.Core.Tests.Events
{
    public class EventBusTests
    {
        private readonly IEventBus sut;
        private readonly IServiceLocator serviceLocator;
        private readonly IEventListener<TestEvent> listener1;
        private readonly IEventListener<TestEvent> listener2;

        public EventBusTests()
        {
            serviceLocator = Substitute.For<IServiceLocator>();
            sut = new EventBus(serviceLocator);

            listener1 = Substitute.For<IEventListener<TestEvent>>();
            listener2 = Substitute.For<IEventListener<TestEvent>>();
        }

        [Fact]
        public async Task SendAsync_SelectsCorrectHandler()
        {
            serviceLocator.GetAll(typeof(IEventListener<TestEvent>)).Returns(new[] { listener1, listener2 });

            var eventMessage = new EventMessage<TestEvent>(new TestEvent(), new Dictionary<string, string>());
            var cancellationToken = new CancellationToken();

            await sut.PublishAsync(eventMessage, cancellationToken);

            listener1.Received(1).HandleAsync(eventMessage, cancellationToken);
            listener2.Received(1).HandleAsync(eventMessage, cancellationToken); // TODO test better that has been awaited?
        }

        public class TestEvent : IEvent
        {
        }
    }
}
