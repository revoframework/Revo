using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Events;

namespace Revo.EFCore.Projections
{
    public class EFCoreSyncProjectionHook : ITransactionParticipant
    {
        private readonly ICommandContext commandContext;
        private readonly IEFCoreProjectionSubSystem projectionSubSystem;

        public EFCoreSyncProjectionHook(ICommandContext commandContext, IEFCoreProjectionSubSystem projectionSubSystem)
        {
            this.commandContext = commandContext;
            this.projectionSubSystem = projectionSubSystem;
        }

        public async Task OnBeforeCommitAsync()
        {
            if (commandContext.UnitOfWork != null)
            {
                await projectionSubSystem.ExecuteProjectionsAsync(
                    commandContext.UnitOfWork.EventBuffer.Events
                        .OfType<IEventMessage<DomainAggregateEvent>>()
                        .ToArray(),
                    commandContext.UnitOfWork,
                    new EFCoreEventProjectionOptions(true));
            }
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
