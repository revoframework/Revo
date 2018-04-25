using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Integrations.Rebus.Events
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
