using System;
using System.Collections.Generic;
using Revo.Core.Events;

namespace Revo.EasyNetQ.Configuration
{
    public class EasyNetQEventTransportsConfiguration
    {
        public Dictionary<Type, Type> Events { get; set; } = new Dictionary<Type, Type>();

        public EasyNetQEventTransportsConfiguration AddType(Type eventType, Type publishAsEventType = null)
        {
            if (!typeof(IEvent).IsAssignableFrom(eventType))
            {
                throw new ArgumentException($"Cannot setup EasyNetQ event transport for type {eventType} because it does not implement {nameof(IEvent)}");
            }

            if (!typeof(IEvent).IsAssignableFrom(publishAsEventType))
            {
                throw new ArgumentException($"Cannot setup EasyNetQ event transport publishing event type {publishAsEventType} because it does not implement {nameof(IEvent)}");
            }

            Events.Add(eventType, publishAsEventType ?? eventType);
            return this;
        }

        public EasyNetQEventTransportsConfiguration AddType<TEvent, TPublishAsEvent>()
            where TEvent : TPublishAsEvent
            where TPublishAsEvent : IEvent
        {
            return AddType(typeof(TEvent), typeof(TPublishAsEvent));
        }
    }
}
