using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.EF6.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.EF6.EventStore.Model;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventStore;

namespace Revo.Infrastructure.EF6.EventStore
{
    public class EF6EventSourceCatchUp : IEventSourceCatchUp
    {
        private readonly IEF6CrudRepository ef6CrudRepository;
        private readonly IAsyncEventQueueDispatcher asyncEventQueueDispatcher;
        private readonly IDomainEventTypeCache domainEventTypeCache;

        public EF6EventSourceCatchUp(IEF6CrudRepository ef6CrudRepository,
            IAsyncEventQueueDispatcher asyncEventQueueDispatcher,
            IDomainEventTypeCache domainEventTypeCache)
        {
            this.ef6CrudRepository = ef6CrudRepository;
            this.asyncEventQueueDispatcher = asyncEventQueueDispatcher;
            this.domainEventTypeCache = domainEventTypeCache;
        }

        public async Task CatchUpAsync()
        {
            var nondispatchedEvents = (await ef6CrudRepository
                .Where<EventStreamRow>(x => !x.IsDispatchedToAsyncQueues)
                .OrderBy(x => x.StreamId)
                .ThenBy(x => x.StreamSequenceNumber)
                .ToListAsync())
                .Select(x =>
                {
                    x.DomainEventTypeCache = domainEventTypeCache;
                    return x;
                }).ToList();

            if (nondispatchedEvents.Count > 0)
            {
                var messages = nondispatchedEvents.Select(EventStoreEventMessage.FromRecord);
                await asyncEventQueueDispatcher.DispatchToQueuesAsync(messages, null, null);
            }
        }
    }
}
