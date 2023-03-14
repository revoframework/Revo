using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Async.Generic;

namespace Revo.Infrastructure.EventStores.Generic
{
    public class ExternalEventSourceCatchUp : IEventSourceCatchUp
    {
        private readonly IReadRepository readRepository;
        private readonly IAsyncEventQueueDispatcher asyncEventQueueDispatcher;
        private readonly IEventSerializer eventSerializer;

        public ExternalEventSourceCatchUp(IReadRepository readRepository,
            IAsyncEventQueueDispatcher asyncEventQueueDispatcher,
            IEventSerializer eventSerializer)
        {
            this.readRepository = readRepository;
            this.asyncEventQueueDispatcher = asyncEventQueueDispatcher;
            this.eventSerializer = eventSerializer;
        }

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
