using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.FluentConfiguration;
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
        private IEasyNetQSubscriptionHandler subscriptionHandler;

        public BusInitializerTests()
        {
            configurationSection = new EasyNetQConfigurationSection();
            bus = Substitute.For<IBus>();
            subscriptionHandler = Substitute.For<IEasyNetQSubscriptionHandler>();

            sut = new BusInitializer(bus, subscriptionHandler, configurationSection);
        }

        [Fact]
        public void OnApplicationStarted_Subscribes()
        {
            configurationSection.Subscriptions
                .AddType<Event1>("sub1", null)
                .AddType<Event2>("sub2", null);

            sut.OnApplicationStarted();
            
            bus.Received(1).SubscribeAsync<IEventMessage<Event1>>("sub1", Arg.Any<Func<IEventMessage<Event1>, Task>>(), Arg.Any<Action<ISubscriptionConfiguration>>());
            bus.Received(1).SubscribeAsync<IEventMessage<Event2>>("sub2", Arg.Any<Func<IEventMessage<Event2>, Task>>(), Arg.Any<Action<ISubscriptionConfiguration>>());
        }

        [Fact]
        public void OnApplicationStarted_HandleMessage()
        {
            configurationSection.Subscriptions
                .AddType<Event1>("sub1", null);

            Dictionary<string, Func<IEventMessage<Event1>, Task>> handleActions = new Dictionary<string, Func<IEventMessage<Event1>, Task>>();
            bus.WhenForAnyArgs(x => x.SubscribeAsync<IEventMessage<Event1>>(null, null, null))
                .Do(ci => handleActions[ci.ArgAt<string>(0)] = ci.ArgAt<Func<IEventMessage<Event1>, Task>>(1));
            
            sut.OnApplicationStarted();

            var subCfg1 = Substitute.For<ISubscriptionConfiguration>();

            var message = Substitute.For<IEventMessage<Event1>>();
            handleActions["sub1"](message);
            subscriptionHandler.Received(1).HandleMessage(message);
        }

        [Fact]
        public void OnApplicationStarted_ConfigurationAction()
        {
            var cfgAction1 = Substitute.For<Action<ISubscriptionConfiguration>>();
            configurationSection.Subscriptions
                .AddType<Event1>("sub1", cfgAction1)
                .AddType<Event2>("sub2", null);
            
            Dictionary<string, Action<ISubscriptionConfiguration>> configActions = new Dictionary<string, Action<ISubscriptionConfiguration>>();
            bus.WhenForAnyArgs(x => x.SubscribeAsync<IEventMessage<Event1>>(null, null, null))
                .Do(ci => configActions[ci.ArgAt<string>(0)] = ci.ArgAt<Action<ISubscriptionConfiguration>>(2));
            bus.WhenForAnyArgs(x => x.SubscribeAsync<IEventMessage<Event2>>(null, null, null))
                .Do(ci => configActions[ci.ArgAt<string>(0)] = ci.ArgAt<Action<ISubscriptionConfiguration>>(2));
            
            sut.OnApplicationStarted();

            var subCfg1 = Substitute.For<ISubscriptionConfiguration>();
            configActions["sub1"](subCfg1);
            cfgAction1.Received(1).Invoke(subCfg1);
            
            var subCfg2 = Substitute.For<ISubscriptionConfiguration>();
            configActions["sub2"](subCfg2);
        }

        public class Event1 : IEvent
        {
        }

        public class Event2 : IEvent
        {
        }
    }
}
