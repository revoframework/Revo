using System;
using System.Collections.Generic;
using EasyNetQ;
using Revo.Core.Events;

namespace Revo.EasyNetQ.Configuration
{
    public class EasyNetQSubscriptionsConfiguration
    {
        public Dictionary<Type, SubscriptionConfiguration> Events { get; set; } = new Dictionary<Type, SubscriptionConfiguration>();
        
        /// <summary>
        /// Adds a subscription for an event type.
        /// Once subscribed, the received events will get pushed to event listeners via the event bus.
        /// </summary>
        /// <param name="eventType">
        /// Type (or sub-type) to subscribe for. This must match the exact type how the events are exported
        /// by the corresponding EventTransport in the publishing side, because it determines the RabbitMQ exchange name.
        /// </param>
        /// <param name="subscriptionId">Subscription name. In combination with the eventType, this determines the resulting RabbitMQ queue name.</param>
        /// <param name="configurationAction">Additional configuration for the EasyNetQ subscription (optional).</param>
        /// <param name="isBlockingSubscriber">
        /// If subscribed as blocking, events from the queue will get processed sequentially (i.e. waiting for the event listeners
        /// to complete before processing another message) ensuring original message ordering. Default is false (concurrent processing).
        /// </param>
        public EasyNetQSubscriptionsConfiguration AddType(Type eventType,
            string subscriptionId,
            Action<ISubscriptionConfiguration> configurationAction = null,
            bool isBlockingSubscriber = false)
        {
            if (!typeof(IEvent).IsAssignableFrom(eventType))
            {
                throw new ArgumentException($"Cannot subscribe to EasyNetQ event type {eventType} because it does not implement {nameof(IEvent)}");
            }

            Events.Add(eventType, new SubscriptionConfiguration()
            {
                ConfigurationAction = configurationAction,
                SubscriptionId = subscriptionId,
                IsBlockingSubscriber = isBlockingSubscriber
            });
            return this;
        }

        /// <summary>
        /// Adds a subscription for an event type.
        /// Once subscribed, the received events will get pushed to event listeners via the event bus.
        /// </summary>
        /// <typeparam name="T">
        /// Type (or sub-type) to subscribe for. This must match the exact type how the events are exported
        /// by the corresponding EventTransport in the publishing side, because it determines the RabbitMQ exchange name.
        /// </typeparam>
        /// <param name="subscriptionId">Subscription name. In combination with the eventType, this determines the resulting RabbitMQ queue name.</param>
        /// <param name="configurationAction">Additional configuration for the EasyNetQ subscription (optional).</param>
        /// <param name="isBlockingSubscriber">
        /// If subscribed as blocking, events from the queue will get processed sequentially (i.e. waiting for the event listeners
        /// to complete before processing another message) ensuring original message ordering. Default is false (concurrent processing).
        /// </param>
        public EasyNetQSubscriptionsConfiguration AddType<T>(string subscriptionId,
            Action<ISubscriptionConfiguration> configurationAction = null,
            bool isBlockingSubscriber = false) where T : IEvent
        {
            return AddType(typeof(T), subscriptionId, configurationAction, isBlockingSubscriber);
        }

        public class SubscriptionConfiguration
        {
            public Action<ISubscriptionConfiguration> ConfigurationAction { get; set; }
            public string SubscriptionId { get; set; }
            public bool IsBlockingSubscriber { get; set; }
        }
    }
}
