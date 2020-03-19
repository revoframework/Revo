using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Projections
{
    /// <summary>
    /// An event projector for an aggregate type.
    /// </summary>
    public interface IEntityEventProjector
    {
        Task ProjectEventsAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events);
        Task CommitChangesAsync();
    }
}
