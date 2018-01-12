using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.Events;
using GTRevo.Infrastructure.EventStore;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcedAggregateRepository : EventSourcedRepository<IEventSourcedAggregateRoot>, IEventSourcedAggregateRepository
    {
        public EventSourcedAggregateRepository(IEventStore eventStore,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer,
            IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory)
            : base(eventStore, entityTypeManager, publishEventBuffer, repositoryFilters, eventMessageFactory)
        {
        }

        protected EventSourcedAggregateRepository(IEventStore eventStore,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer,
            IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory,
            Dictionary<Guid, IEventSourcedAggregateRoot> aggregates)
            : base(eventStore, entityTypeManager, publishEventBuffer, repositoryFilters, eventMessageFactory, aggregates)
        {
        }

        protected override EventSourcedRepository<IEventSourcedAggregateRoot> CloneWithFilters(
            IEventStore eventStore,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer,
            IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory,
            Dictionary<Guid, IEventSourcedAggregateRoot> aggregates)
        {
            return new EventSourcedAggregateRepository(eventStore,
                entityTypeManager,
                publishEventBuffer,
                repositoryFilters,
                eventMessageFactory,
                aggregates);
        }
    }
}
