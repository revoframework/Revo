using System;
using System.Collections.Generic;
using System.Linq;

namespace Revo.Core.Events
{
    public static class EventMessageDraft
    {
        public static IEventMessageDraft<TEvent> FromEvent<TEvent>(TEvent @event)
            where TEvent : IEvent
        {
            Type messageType = typeof(EventMessageDraft<>).MakeGenericType(@event.GetType());
            return (IEventMessageDraft<TEvent>)messageType.GetConstructor(new[] {@event.GetType()})
                .Invoke(new object[] { @event });
        }
    }

    public class EventMessageDraft<TEvent> : IEventMessageDraft<TEvent>
        where TEvent : IEvent
    {
        private readonly Dictionary<string, string> metadata;

        public EventMessageDraft(TEvent @event)
        {
            Event = @event;
            metadata = new Dictionary<string, string>();
        }
        
        public EventMessageDraft(TEvent @event, IReadOnlyDictionary<string, string> metadata)
        {
            Event = @event;
            this.metadata = metadata.ToDictionary(x => x.Key, x => x.Value);
        }

        IEvent IEventMessage.Event => Event;

        public TEvent Event { get; }
        public IReadOnlyDictionary<string, string> Metadata => metadata;

        public IEventMessageDraft SetMetadata(string key, string value)
        {
            metadata[key] = value;
            return this;
        }
    }
}
