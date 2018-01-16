using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.EF6.EventStore.Model;
using GTRevo.Infrastructure.Events.Async;
using GTRevo.Infrastructure.EventStore;

namespace GTRevo.Infrastructure.EF6.EventStore
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
