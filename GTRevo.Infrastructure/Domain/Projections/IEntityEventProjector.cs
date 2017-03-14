using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Infrastructure.EventSourcing;

namespace GTRevo.Infrastructure.Domain.Projections
{
    public interface IEntityEventProjector
    {
        Type ProjectedAggregateType { get; }

        Task ProjectEventsAsync(IEventSourcedAggregateRoot aggregate,
            IEnumerable<DomainAggregateEvent> events);
        Task CommitChangesAsync();
    }

    public interface IEntityEventProjector<in T> : IEntityEventProjector
        where T : IAggregateRoot
    {
        Task ProjectEventsAsync(T aggregate, IEnumerable<DomainAggregateEvent> events);
    }
}
