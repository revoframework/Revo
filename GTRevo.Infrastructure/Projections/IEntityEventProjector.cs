using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;

namespace GTRevo.Infrastructure.Projections
{
    public interface IEntityEventProjector // TODO entity event projectors for non-event-sourced entities
    {
        Type ProjectedAggregateType { get; }

        Task ProjectEventsAsync(IEventSourcedAggregateRoot aggregate,
            IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events);
        Task CommitChangesAsync();
    }

    public interface IEntityEventProjector<in T> : IEntityEventProjector
        where T : IEventSourcedAggregateRoot
    {
        Task ProjectEventsAsync(T aggregate, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events);
    }
}
