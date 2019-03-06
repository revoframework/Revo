using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Core;
using Revo.Core.Events;
using Xunit;

namespace Revo.EasyNetQ.Tests
{
    public class EasyNetQSubscriptionHandlerTests
    {
        private IEventBus eventBus;
        private EasyNetQSubscriptionHandler sut;

        public EasyNetQSubscriptionHandlerTests()
        {
            eventBus = Substitute.For<IEventBus>();
            sut = new EasyNetQSubscriptionHandler(eventBus);
        }

        [Fact]
        public async Task HandleMessage()
        {
            var eventMessage = EventMessage.FromEvent(new Event1(), new Dictionary<string, string>());

            var prevTaskContext = TaskContext.Current;
            TaskContext currentTaskContext = null;

            eventBus.When(x => x.PublishAsync(eventMessage)).Do(ci => currentTaskContext = TaskContext.Current);

            await sut.HandleMessage(eventMessage);

            eventBus.Received(1).PublishAsync(eventMessage);
            currentTaskContext.Should().NotBeNull();
            currentTaskContext.Should().NotBe(prevTaskContext);
        }

        private class Event1 : IEvent
        {
        }
    }
}
