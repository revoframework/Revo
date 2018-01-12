using System.Collections.Generic;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events
{
    public class EventMessageDraft<TEvent> : IEventMessageDraft<TEvent>
        where TEvent : IEvent
    {
        private readonly Dictionary<string, string> metadata = new Dictionary<string, string>();

        public EventMessageDraft(TEvent @event)
        {
            Event = @event;
        }

        IEvent IEventMessage.Event => Event;

        public TEvent Event { get; }
        public IReadOnlyDictionary<string, string> Metadata => metadata;

        public void AddMetadata(string key, string value)
        {
            metadata.Add(key, value);
        }
    }
}
