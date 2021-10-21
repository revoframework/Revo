using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Internals;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Events;
using Revo.EasyNetQ.Configuration;
using Xunit;

namespace Revo.EasyNetQ.Tests
{
    public class BusInitializerTests
    {
        private BusInitializer sut;
        private EasyNetQConfigurationSection configurationSection;
        private IBus bus;
        private IPubSub pubSub;
        private IEasyNetQSubscriptionHandler subscriptionHandler;
        private IEasyNetQBlockingSubscriptionHandler blockingSubscriptionHandler;

        public BusInitializerTests()
        {
            configurationSection = new EasyNetQConfigurationSection();
            bus = Substitute.For<IBus>();
            pubSub = Substitute.For<IPubSub>();
            pubSub.SubscribeAsync<IEventMessage<Event1>>(null, null, null, CancellationToken.None)
                .ReturnsForAnyArgs(new AwaitableDisposable<ISubscriptionResult>(Task.FromResult<ISubscriptionResult>(null)));
            pubSub.SubscribeAsync<IEventMessage<Event2>>(null, null, null, CancellationToken.None)
                .ReturnsForAnyArgs(new AwaitableDisposable<ISubscriptionResult>(Task.FromResult<ISubscriptionResult>(null)));
            bus.PubSub.Returns(pubSub);
            subscriptionHandler = Substitute.For<IEasyNetQSubscriptionHandler>();
            blockingSubscriptionHandler = Substitute.For<IEasyNetQBlockingSubscriptionHandler>();

            sut = new BusInitializer(bus, subscriptionHandler, blockingSubscriptionHandler,
                configurationSection);
        }

        [Fact]
        public void OnApplicationStarted_Subscribes()
        {
            configurationSection.Subscriptions
                .AddType<Event1>("sub1", null)
                .AddType<Event2>("sub2", null);

            sut.OnApplicationStarted();

            pubSub.Received(1).SubscribeAsync("sub1", Arg.Any<Func<IEventMessage<Event1>, CancellationToken, Task>>(),
                Arg.Any<Action<ISubscriptionConfiguration>>(), Arg.Any<CancellationToken>());
            pubSub.Received(1).SubscribeAsync("sub2", Arg.Any<Func<IEventMessage<Event2>, CancellationToken, Task>>(),
                Arg.Any<Action<ISubscriptionConfiguration>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task OnApplicationStarted_SubscribesBlocking()
        {
            configurationSection.Subscriptions
                .AddType<Event1>("sub1", null, true)
                .AddType<Event2>("sub2", null, true);

            sut.OnApplicationStarted();
            
            pubSub.Received(1).SubscribeAsync("sub1", Arg.Any<Func<IEventMessage<Event1>, CancellationToken, Task>>(),
                Arg.Any<Action<ISubscriptionConfiguration>>(), Arg.Any<CancellationToken>());
            pubSub.Received(1).SubscribeAsync("sub2", Arg.Any<Func<IEventMessage<Event2>, CancellationToken, Task>>(),
                Arg.Any<Action<ISubscriptionConfiguration>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public void OnApplicationStarted_HandleMessage()
        {
            configurationSection.Subscriptions
                .AddType<Event1>("sub1", null);

            Dictionary<string, Func<IEventMessage<Event1>, CancellationToken, Task>> handleActions = new();
            pubSub.WhenForAnyArgs(x => x.SubscribeAsync<IEventMessage<Event1>>(null, null, null))
                .Do(ci => handleActions[ci.ArgAt<string>(0)] = ci.ArgAt<Func<IEventMessage<Event1>, CancellationToken, Task>>(1));
            
            sut.OnApplicationStarted();
            
            var message = Substitute.For<IEventMessage<Event1>>();
            handleActions["sub1"](message, CancellationToken.None);
            subscriptionHandler.Received(1).HandleMessageAsync(message);
        }

        [Fact]
        public void OnApplicationStarted_ConfigurationAction()
        {
            var cfgAction1 = Substitute.For<Action<ISubscriptionConfiguration>>();
            configurationSection.Subscriptions
                .AddType<Event1>("sub1", cfgAction1);
            
            Dictionary<string, Action<ISubscriptionConfiguration>> configActions = new Dictionary<string, Action<ISubscriptionConfiguration>>();
            pubSub.WhenForAnyArgs(x => x.SubscribeAsync<IEventMessage<Event1>>(null, null, null))
                .Do(ci => configActions[ci.ArgAt<string>(0)] = ci.ArgAt<Action<ISubscriptionConfiguration>>(2));
            
            sut.OnApplicationStarted();

            var subCfg1 = Substitute.For<ISubscriptionConfiguration>();
            configActions["sub1"](subCfg1);
            cfgAction1.Received(1).Invoke(subCfg1);
        }

        [Fact]
        public void OnApplicationStarted_ConfigurationAction_Null()
        {
            configurationSection.Subscriptions
                .AddType<Event1>("sub1", null);
            
            Dictionary<string, Action<ISubscriptionConfiguration>> configActions = new Dictionary<string, Action<ISubscriptionConfiguration>>();
            pubSub.WhenForAnyArgs(x => x.SubscribeAsync<IEventMessage<Event1>>(null, null, null))
                .Do(ci => configActions[ci.ArgAt<string>(0)] = ci.ArgAt<Action<ISubscriptionConfiguration>>(2));
            
            sut.OnApplicationStarted();
            
            configActions["sub1"].Should().NotBeNull();
        }

        public class Event1 : IEvent
        {
        }

        public class Event2 : IEvent
        {
        }
    }
}
