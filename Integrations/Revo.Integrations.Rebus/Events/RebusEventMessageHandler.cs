using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rebus.Handlers;
using Rebus.Pipeline;
using Revo.Core.Events;

namespace Revo.Integrations.Rebus.Events
{
    public class RebusEventMessageHandler : IHandleMessages<IEvent>
    {
        private readonly IMessageContext messageContext;

        public RebusEventMessageHandler(IMessageContext messageContext)
        {
            this.messageContext = messageContext;
        }

        public Task Handle(IEvent message)
        {
            var uow = (RebusUnitOfWork) messageContext.TransactionContext.Items["uow"];
            uow.AddMessage(message, messageContext.Headers);
            return Task.FromResult(0);
        }
    }
}
