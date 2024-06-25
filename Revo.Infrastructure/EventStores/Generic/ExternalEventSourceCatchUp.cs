using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Async.Generic;

namespace Revo.Infrastructure.EventStores.Generic
{
    public class ExternalEventSourceCatchUp(IReadRepository readRepository,
            IAsyncEventQueueDispatcher asyncEventQueueDispatcher,
            IEventSerializer eventSerializer) : IEventSourceCatchUp
    {
        public async Task CatchUpAsync()
        {
            var nondispatchedEvents = (await readRepository
                .Where<ExternalEventRecord>(x => !x.IsDispatchedToAsyncQueues)
                .ToListAsync(readRepository));

            if (nondispatchedEvents.Count > 0)
            {
                var messages = nondispatchedEvents.Select(x =>
                    ExternalEventMessageAdapter.FromRecord(x, eventSerializer));

                await asyncEventQueueDispatcher.DispatchToQueuesAsync(messages, null, null);
            }
        }
    }
}
