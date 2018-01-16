using System.Collections.Generic;

namespace GTRevo.Core.Events
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

        public void SetMetadata(string key, string value)
        {
            metadata[key] = value;
        }
    }
}
