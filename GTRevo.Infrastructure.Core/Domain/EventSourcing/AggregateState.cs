using System.Collections.Generic;
using GTRevo.Infrastructure.Domain.Events;

namespace GTRevo.Infrastructure.Domain.EventSourcing
{
    public class AggregateState
    {
        public AggregateState(int version, IEnumerable<DomainAggregateEvent> events,
            bool isDeleted)
        {
            Version = version;
            IsDeleted = isDeleted;
            Events = events;
        }

        public int Version { get; private set; }
        public bool IsDeleted { get; private set; }
        public IEnumerable<DomainAggregateEvent> Events { get; private set; }
    }
}
