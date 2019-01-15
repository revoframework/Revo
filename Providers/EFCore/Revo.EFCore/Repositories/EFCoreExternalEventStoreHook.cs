using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Transactions;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.EventStores.Generic;

namespace Revo.EFCore.Repositories
{
    public class EFCoreExternalEventStoreHook : ITransactionParticipant
    {
        private readonly ICommandContext commandContext;
        private readonly IExternalEventStore externalEventStore;

        public EFCoreExternalEventStoreHook(ICommandContext commandContext, IExternalEventStore externalEventStore)
        {
            this.commandContext = commandContext;
            this.externalEventStore = externalEventStore;
        }

        public Task OnBeforeCommitAsync()
        {
            if (commandContext.UnitOfWork != null)
            {
                foreach (var ev in commandContext.UnitOfWork.EventBuffer.Events)
                {
                    if (!((ev as IEventStoreEventMessage)?.Record is EventStoreRecordAdapter))
                    {
                        externalEventStore.PushEvent(ev);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task OnCommitSucceededAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnCommitFailedAsync()
        {
            return Task.CompletedTask;
        }
    }
}
