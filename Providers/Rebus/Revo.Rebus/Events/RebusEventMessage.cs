using System.Collections.Generic;
using Revo.Core.Events;

namespace Revo.Rebus.Events
{
    public class RebusEventMessage<TEvent> : IEventMessage<TEvent> where TEvent : IEvent
    {
        public RebusEventMessage(TEvent @event, IReadOnlyDictionary<string, string> metadata)
        {
            Event = @event;
            Metadata = metadata;
        }

        IEvent IEventMessage.Event => Event;

        public TEvent Event { get; }
        public IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
