using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Core.Domain.EventSourcing
{
    public interface IAggregateHistory
    {
        Guid AggregateId { get; }
        Guid AggregateClassId { get; }
        int Version { get; }

        Task<IEnumerable<DomainAggregateEventRecord>> GetEventsAsync();
    }
}
