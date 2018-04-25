using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Integrations.Rebus.Events;

namespace Revo.Integrations.Rebus
{
    public class RebusUnitOfWork
    {
        private readonly Func<ICommandBus> commandBusFunc;
        private readonly List<RebusEventMessage<IEvent>> messages = new List<RebusEventMessage<IEvent>>();

        public RebusUnitOfWork(Func<ICommandBus> commandBusFunc)
        {
            this.commandBusFunc = commandBusFunc;
        }

        public IReadOnlyCollection<RebusEventMessage<IEvent>> Messages => messages;

        public void AddMessage(IEvent message, Dictionary<string, string> headers)
        {
            messages.Add(new RebusEventMessage<IEvent>(message, headers));
        }

        public async Task RunAsync()
        {
            await Task.Factory.StartNewWithContext(async () =>
            {
                var commandBus = commandBusFunc();
                await commandBus.SendAsync(new HandleRebusEventsCommand(messages.ToImmutableList()));
            });
        }
    }
}
