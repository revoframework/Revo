using System.Collections.Generic;
using System.Linq;
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
        private readonly List<IEventMessage> projectedEvents = new List<IEventMessage>();

        public EFCoreSyncProjectionHook(ICommandContext commandContext, IEFCoreProjectionSubSystem projectionSubSystem)
        {
            this.commandContext = commandContext;
            this.projectionSubSystem = projectionSubSystem;
        }

        public async Task OnBeforeCommitAsync()
        {
            if (commandContext.UnitOfWork != null)
            {
                var newEvents = commandContext.UnitOfWork.EventBuffer.Events
                    .SkipWhile((x, i) => projectedEvents.Count > i && projectedEvents[i] == x)
                    .ToArray();

                if (newEvents.Length > 0)
                {
                    projectedEvents.AddRange(newEvents);

                    await projectionSubSystem.ExecuteProjectionsAsync(
                        newEvents
                            .OfType<IEventMessage<DomainAggregateEvent>>()
                            .ToArray(),
                        commandContext.UnitOfWork,
                        new EFCoreEventProjectionOptions(true));
                }
            }
        }

        public Task OnCommitSucceededAsync()
        {
            projectedEvents.Clear();
            return Task.CompletedTask;
        }

        public Task OnCommitFailedAsync()
        {
            projectedEvents.Clear();
            return Task.CompletedTask;
        }
    }
}
