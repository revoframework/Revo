using System.Collections.Generic;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Core.Domain.EventSourcing
{
    public class AggregateState
    {
        public AggregateState(int version, IReadOnlyCollection<DomainAggregateEvent> events)
        {
            Version = version;
            Events = events;
        }

        public int Version { get; private set; }
        public IReadOnlyCollection<DomainAggregateEvent> Events { get; private set; }
    }
}
