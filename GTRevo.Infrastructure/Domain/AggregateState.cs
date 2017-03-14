using System.Collections.Generic;

namespace GTRevo.Infrastructure.Domain
{
    public class AggregateState
    {
        public AggregateState(int version, IEnumerable<DomainAggregateEvent> events)
        {
            Version = version;
            Events = events;
        }

        public int Version { get; private set; }
        public IEnumerable<DomainAggregateEvent> Events { get; private set; }
    }
}
