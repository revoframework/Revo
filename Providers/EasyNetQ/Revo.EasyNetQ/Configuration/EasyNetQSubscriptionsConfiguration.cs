using System;
using System.Collections.Generic;
using EasyNetQ.FluentConfiguration;
using Revo.Core.Events;

namespace Revo.EasyNetQ.Configuration
{
    public class EasyNetQSubscriptionsConfiguration
    {
        public Dictionary<Type, SubscriptionConfiguration> Events { get; set; } = new Dictionary<Type, SubscriptionConfiguration>();

        public EasyNetQSubscriptionsConfiguration AddType(Type eventType,
            string subscriptionId,
            Action<ISubscriptionConfiguration> configurationAction = null)
        {
            if (!typeof(IEvent).IsAssignableFrom(eventType))
            {
                throw new ArgumentException($"Cannot subscribe to EasyNetQ event type {eventType} because it does not implement {nameof(IEvent)}");
            }

            Events.Add(eventType, new SubscriptionConfiguration()
            {
                ConfigurationAction = configurationAction,
                SubscriptionId = subscriptionId
            });
            return this;
        }

        public EasyNetQSubscriptionsConfiguration AddType<T>(string subscriptionId,
            Action<ISubscriptionConfiguration> configurationAction = null) where T : IEvent
        {
            return AddType(typeof(T), subscriptionId, configurationAction);
        }

        public class SubscriptionConfiguration
        {
            public Action<ISubscriptionConfiguration> ConfigurationAction { get; set; }
            public string SubscriptionId { get; set; }
        }
    }
}
