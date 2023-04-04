using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Events;
using NSubstitute;
using Xunit;

namespace Revo.Core.Tests.Events
{
    public class EventBusTests
    {
        private readonly IEventBus sut;
        private readonly IServiceProvider serviceProvider;
        private readonly IEventListener<TestEvent> listener1;
        private readonly IEventListener<TestEvent> listener2;

        public EventBusTests()
        {
            serviceProvider = Substitute.For<IServiceProvider>();
            sut = new EventBus(serviceProvider);

            listener1 = Substitute.For<IEventListener<TestEvent>>();
            listener2 = Substitute.For<IEventListener<TestEvent>>();

            serviceProvider.GetService(typeof(IEnumerable<IEventListener<TestEvent>>))
                .Returns(new IEventListener<TestEvent>[] { listener1, listener2 });
        }

        [Fact]
        public async Task SendAsync_SelectsCorrectHandler()
        {
            var eventMessage = new EventMessage<TestEvent>(new TestEvent(), new Dictionary<string, string>());
            var cancellationToken = new CancellationToken();

            await sut.PublishAsync(eventMessage, cancellationToken);

            listener1.Received(1).HandleAsync(eventMessage, cancellationToken);
            listener2.Received(1).HandleAsync(eventMessage, cancellationToken);
        }

        public class TestEvent : IEvent
        {
        }
    }
}