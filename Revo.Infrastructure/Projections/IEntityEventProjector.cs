using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Projections
{
    /// <summary>
    /// An event projector for an aggregate type.
    /// </summary>
    public interface IEntityEventProjector // TODO entity event projectors for non-event-sourced entities
    {
        Type ProjectedAggregateType { get; }

        Task ProjectEventsAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events);
        Task CommitChangesAsync();
    }

    /// <summary>
    /// An event projector for an aggregate type.
    /// </summary>
    /// <typeparam name="T">Aggregate type.</typeparam>
    public interface IEntityEventProjector<T> : IEntityEventProjector
        where T : IAggregateRoot
    {
    }
}
