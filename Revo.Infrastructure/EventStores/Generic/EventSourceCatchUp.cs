using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventStores.Generic.Model;

namespace Revo.Infrastructure.EventStores.Generic
{
    public class EventSourceCatchUp : IEventSourceCatchUp
    {
        private readonly IReadRepository readRepository;
        private readonly IAsyncEventQueueDispatcher asyncEventQueueDispatcher;
        private readonly IEventSerializer eventSerializer;

        public EventSourceCatchUp(IReadRepository readRepository,
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
                .Where<EventStreamRow>(x => !x.IsDispatchedToAsyncQueues)
                .OrderBy(x => x.StreamId)
                .ThenBy(x => x.StreamSequenceNumber)
                .ToListAsync(readRepository))
                .Select(x =>
                    new EventStoreRecordAdapter(x, eventSerializer)).ToList();

            if (nondispatchedEvents.Count > 0)
            {
                var messages = nondispatchedEvents.Select(EventStoreEventMessage.FromRecord);
                await asyncEventQueueDispatcher.DispatchToQueuesAsync(messages, null, null);
            }
        }
    }
}
