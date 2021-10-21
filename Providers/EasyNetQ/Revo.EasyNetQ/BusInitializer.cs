using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Revo.Core.Events;
using Revo.Core.Lifecycle;
using Revo.EasyNetQ.Configuration;

namespace Revo.EasyNetQ
{
    public class BusInitializer : IApplicationStartedListener,
        IApplicationStoppingListener
    {
        private static readonly MethodInfo SubscribeBlockingMethod = typeof(BusInitializer)
            .GetMethod(nameof(SubscribeBlocking), BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo SubscribeMethod = typeof(BusInitializer)
            .GetMethod(nameof(Subscribe), BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly IBus bus;
        private readonly IEasyNetQSubscriptionHandler subscriptionHandler;
        private readonly IEasyNetQBlockingSubscriptionHandler blockingSubscriptionHandler;
        private readonly EasyNetQConfigurationSection configurationSection;

        public BusInitializer(IBus bus, IEasyNetQSubscriptionHandler subscriptionHandler,
            IEasyNetQBlockingSubscriptionHandler blockingSubscriptionHandler,
            EasyNetQConfigurationSection configurationSection)
        {
            this.bus = bus;
            this.subscriptionHandler = subscriptionHandler;
            this.blockingSubscriptionHandler = blockingSubscriptionHandler;
            this.configurationSection = configurationSection;
        }

        public void OnApplicationStarted()
        {
            foreach (var eventType in configurationSection.Subscriptions.Events)
            {
                if (eventType.Value.IsBlockingSubscriber)
                {
                    var subscribeTask = (Task) SubscribeBlockingMethod.MakeGenericMethod(eventType.Key).Invoke(this, new[] {eventType.Value});
                    subscribeTask.GetAwaiter().GetResult();
                }
                else
                {
                    var subscribeTask = (Task)SubscribeMethod.MakeGenericMethod(eventType.Key).Invoke(this, new[] { eventType.Value });
                    subscribeTask.GetAwaiter().GetResult();
                }
            }
        }

        public void OnApplicationStopping()
        {
        }

        private async Task SubscribeBlocking<T>(EasyNetQSubscriptionsConfiguration.SubscriptionConfiguration subscriptionConfiguration)
            where T : class, IEvent
        {
            Func<IEventMessage<T>, CancellationToken, Task> onMessage = (e, c) =>
            {
                blockingSubscriptionHandler.HandleMessage(e);
                return Task.CompletedTask;
            };

            await bus.PubSub.SubscribeAsync(subscriptionConfiguration.SubscriptionId,
                onMessage,
                subscriptionConfiguration.ConfigurationAction ?? (_ => { }));
        }

        private async Task Subscribe<T>(EasyNetQSubscriptionsConfiguration.SubscriptionConfiguration subscriptionConfiguration)
            where T : class, IEvent
        {
            await bus.PubSub.SubscribeAsync<IEventMessage<T>>(subscriptionConfiguration.SubscriptionId,
                (e, c) => subscriptionHandler.HandleMessageAsync(e),
                subscriptionConfiguration.ConfigurationAction ?? (_ => { }));
        }
    }
}
