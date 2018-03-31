using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Projections
{
    public interface IEntityEventProjector // TODO entity event projectors for non-event-sourced entities
    {
        Type ProjectedAggregateType { get; }

        Task ProjectEventsAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events);
        Task CommitChangesAsync();
    }

    public interface IEntityEventProjector<T> : IEntityEventProjector
        where T : IEventSourcedAggregateRoot
    {
    }
}
