using System.Collections.Generic;
using Revo.Domain.Events;

namespace Revo.Domain.Entities.EventSourcing
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
