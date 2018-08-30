using System.Collections.Immutable;
using Revo.Core.Commands;
using Revo.Core.Events;

namespace Revo.Rebus.Events
{
    public class HandleRebusEventsCommand : ICommand
    {
        public HandleRebusEventsCommand(ImmutableList<IEventMessage<IEvent>> messages)
        {
            Messages = messages;
        }

        public ImmutableList<IEventMessage<IEvent>> Messages { get; }
    }
}
