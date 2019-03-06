using EasyNetQ;
using EasyNetQ.NonGeneric;
using Revo.Core.Events;
using Revo.Core.Lifecycle;
using Revo.EasyNetQ.Configuration;

namespace Revo.EasyNetQ
{
    public class BusInitializer :
        IApplicationStartListener,
        IApplicationStopListener
    {
        private readonly IBus bus;
        private readonly IEasyNetQSubscriptionHandler subscriptionHandler;
        private readonly EasyNetQConfigurationSection configurationSection;

        public BusInitializer(IBus bus, IEasyNetQSubscriptionHandler subscriptionHandler,
            EasyNetQConfigurationSection configurationSection)
        {
            this.bus = bus;
            this.subscriptionHandler = subscriptionHandler;
            this.configurationSection = configurationSection;
        }

        public void OnApplicationStarted()
        {
            foreach (var eventType in configurationSection.Subscriptions.Events)
            {
                bus.SubscribeAsync(typeof(IEventMessage<>).MakeGenericType(eventType.Key), eventType.Value.SubscriptionId,
                    subscriptionHandler.HandleMessage, cfg => eventType.Value.ConfigurationAction?.Invoke(cfg));
            }
        }

        public void OnApplicationStopping()
        {
        }
    }
}
