using System;
using System.Collections.Generic;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStore;

namespace Revo.Infrastructure.EventSourcing
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
