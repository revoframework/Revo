using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStores;

namespace Revo.Infrastructure.Repositories;

public class EventSourcedAggregateStoreFactory : IAggregateStoreFactory
{
    private readonly IEventStore eventStore;
    private readonly IEntityTypeManager entityTypeManager;
    private readonly IRepositoryFilter[] repositoryFilters;
    private readonly IEventMessageFactory eventMessageFactory;
    private readonly IEventSourcedAggregateFactory eventSourcedAggregateFactory;

    public EventSourcedAggregateStoreFactory(IEventStore eventStore,
        IEntityTypeManager entityTypeManager, IEnumerable<IRepositoryFilter> repositoryFilters,
        IEventMessageFactory eventMessageFactory, IEventSourcedAggregateFactory eventSourcedAggregateFactory)
    {
        this.eventStore = eventStore;
        this.entityTypeManager = entityTypeManager;
        this.repositoryFilters = repositoryFilters.ToArray();
        this.eventMessageFactory = eventMessageFactory;
        this.eventSourcedAggregateFactory = eventSourcedAggregateFactory;
    }
        
    public bool CanHandleAggregateType(Type aggregateType)
    {
        return typeof(IEventSourcedAggregateRoot).IsAssignableFrom(aggregateType);
    }

    public virtual IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
    {
        return new EventSourcedAggregateStore(eventStore, entityTypeManager,
            unitOfWork?.EventBuffer, repositoryFilters, eventMessageFactory, eventSourcedAggregateFactory);
    }
}
