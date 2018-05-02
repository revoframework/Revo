using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Events;

namespace Revo.Integrations.Rebus.Events
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
