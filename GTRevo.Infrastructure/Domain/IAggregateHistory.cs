using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Domain
{
    public interface IAggregateHistory
    {
        Guid AggregateId { get; }
        Guid AggregateClassId { get; }
        int Version { get; }

        Task<IEnumerable<DomainAggregateEventRecord>> GetEventsAsync();
    }
}
