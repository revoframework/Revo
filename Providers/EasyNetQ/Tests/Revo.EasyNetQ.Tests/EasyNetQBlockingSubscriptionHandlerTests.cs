using System;
using System.Collections.Generic;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Core;
using Revo.Core.Events;
using Xunit;

namespace Revo.EasyNetQ.Tests
{
    public class EasyNetQBlockingSubscriptionHandlerTests
    {
        private IEventBus eventBus;
        private EasyNetQBlockingSubscriptionHandler sut;

        public EasyNetQBlockingSubscriptionHandlerTests()
        {
            eventBus = Substitute.For<IEventBus>();
            sut = new EasyNetQBlockingSubscriptionHandler(eventBus);
        }

        [Fact]
        public void HandleMessage()
        {
            var eventMessage = EventMessage.FromEvent(new Event1(), new Dictionary<string, string>());

            var prevTaskContext = TaskContext.Current;
            TaskContext currentTaskContext = null;

            eventBus.When(x => x.PublishAsync(eventMessage)).Do(ci => currentTaskContext = TaskContext.Current);

            sut.HandleMessage(eventMessage);

            eventBus.Received(1).PublishAsync(eventMessage);
            currentTaskContext.Should().NotBeNull();
            currentTaskContext.Should().NotBe(prevTaskContext);
        }

        [Fact]
        public void HandleMessage_Exception()
        {
            var eventMessage = EventMessage.FromEvent(new Event1(), new Dictionary<string, string>());

            eventBus.When(x => x.PublishAsync(eventMessage)).Throw<TestException>();

            sut.Invoking(x => x.HandleMessage(eventMessage))
                .Should().ThrowExactly<TestException>();
        }

        private class Event1 : IEvent
        {
        }

        private class TestException : Exception
        {
        }
    }
}