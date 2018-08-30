using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Rebus.Events;

namespace Revo.Rebus
{
    public class RebusUnitOfWork
    {
        private readonly Func<ICommandBus> commandBusFunc;
        private readonly Dictionary<IEvent, IEventMessage<IEvent>> messages = new Dictionary<IEvent, IEventMessage<IEvent>>();

        public RebusUnitOfWork(Func<ICommandBus> commandBusFunc)
        {
            this.commandBusFunc = commandBusFunc;
        }

        public IReadOnlyCollection<IEventMessage<IEvent>> Messages => messages.Values;

        public void AddMessage(IEvent message, Dictionary<string, string> headers)
        {
            if (!messages.ContainsKey(message)) //we need to do this because Rebus will call the handler for every base class of the event as well
            {
                AddMessageInternal((dynamic) message, headers);
            }
        }

        public async Task RunAsync()
        {
            await Task.Factory.StartNewWithContext(async () =>
            {
                var commandBus = commandBusFunc();
                await commandBus.SendAsync(new HandleRebusEventsCommand(messages.Values.ToImmutableList()));
            });
        }

        public void AddMessageInternal<TEvent>(TEvent message, Dictionary<string, string> headers)
            where TEvent : class, IEvent
        {
            messages.Add(message, new RebusEventMessage<TEvent>(message, headers));
        }
    }
}
