using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Revo.Core.Events;
using Xunit;

namespace Revo.EasyNetQ.Tests
{
    public class EasyNetQSubscriptionHandlerTests
    {
        private readonly IEventBus eventBus;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly EasyNetQSubscriptionHandler sut;

        public EasyNetQSubscriptionHandlerTests()
        {
            eventBus = Substitute.For<IEventBus>();

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IEventBus)).Returns(eventBus);

            var scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.Returns(serviceProvider);

            serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            serviceScopeFactory.CreateScope().Returns(scope);

            sut = new EasyNetQSubscriptionHandler(serviceScopeFactory);
        }

        [Fact]
        public async Task HandleMessage()
        {
            var eventMessage = EventMessage.FromEvent(new Event1(), new Dictionary<string, string>());

            await sut.HandleMessageAsync(eventMessage);

            await eventBus.Received(1).PublishAsync(eventMessage);
        }

        [Fact]
        public async Task HandleMessage_Exception()
        {
            var eventMessage = EventMessage.FromEvent(new Event1(), new Dictionary<string, string>());
            
            eventBus.When(x => x.PublishAsync(eventMessage)).Throw<TestException>();

            await sut.Invoking(x => x.HandleMessageAsync(eventMessage))
                .Should().ThrowExactlyAsync<TestException>();
        }

        private class Event1 : IEvent
        {
        }

        private class TestException : Exception
        {
        }
    }
}
