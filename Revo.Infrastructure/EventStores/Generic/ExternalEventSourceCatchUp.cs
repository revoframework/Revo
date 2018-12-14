using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Types;
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
                {
                    var @event = eventSerializer.DeserializeEvent(x.EventJson,
                        new VersionedTypeId(x.EventName, x.EventVersion));
                    var metadata = eventSerializer.DeserializeEventMetadata(x.MetadataJson);
                    return EventMessage.FromEvent(@event, metadata);
                });

                await asyncEventQueueDispatcher.DispatchToQueuesAsync(messages, null, null);
            }
        }
    }
}
