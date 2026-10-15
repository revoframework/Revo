using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Revo.Core.Events;
using Xunit;

namespace Revo.EasyNetQ.Tests
{
    public class EasyNetQBlockingSubscriptionHandlerTests
    {
        private readonly IEventBus eventBus;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly EasyNetQBlockingSubscriptionHandler sut;

        public EasyNetQBlockingSubscriptionHandlerTests()
        {
            eventBus = Substitute.For<IEventBus>();

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IEventBus)).Returns(eventBus);

            var scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.Returns(serviceProvider);

            serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            serviceScopeFactory.CreateScope().Returns(scope);

            sut = new EasyNetQBlockingSubscriptionHandler(serviceScopeFactory);
        }

        [Fact]
        public void HandleMessage()
        {
            var eventMessage = EventMessage.FromEvent(new Event1(), new Dictionary<string, string>());

            sut.HandleMessage(eventMessage);

            eventBus.Received(1).PublishAsync(eventMessage);
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