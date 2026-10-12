using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Services;
using Revo.Core.Events;
using NSubstitute;
using Xunit;

namespace Revo.Core.Tests.Events
{
    public class EventBusTests
    {
        private readonly IEventBus sut;
        private readonly IVariantServiceResolver serviceResolver;
        private readonly IEventListener<TestEvent> listener1;
        private readonly IEventListener<TestEvent> listener2;

        public EventBusTests()
        {
            serviceResolver = Substitute.For<IVariantServiceResolver>();
            sut = new EventBus(serviceResolver);

            listener1 = Substitute.For<IEventListener<TestEvent>>();
            listener2 = Substitute.For<IEventListener<TestEvent>>();

            serviceResolver.GetServices(typeof(IEventListener<TestEvent>))
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

        [Fact]
        public async Task PublishAsync_NotifiesListenersOfBaseEventTypes()
        {
            var baseListener = Substitute.For<IEventListener<IEvent>>();
            var services = new ServiceCollection();
            services.AddSingleton(baseListener);

            var eventBus = new EventBus(
                new VariantServiceResolver(services.BuildServiceProvider(), new VariantServiceTypeIndex(services)));

            var eventMessage = new EventMessage<TestEvent>(new TestEvent(), new Dictionary<string, string>());
            var cancellationToken = new CancellationToken();

            await eventBus.PublishAsync(eventMessage, cancellationToken);

            await baseListener.Received(1).HandleAsync(eventMessage, cancellationToken);
        }

        public class TestEvent : IEvent
        {
        }
    }
}