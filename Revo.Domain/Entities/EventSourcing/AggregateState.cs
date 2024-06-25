using System.Collections.Generic;
using Revo.Domain.Events;

namespace Revo.Domain.Entities.EventSourcing
{
    public class AggregateState(int version, IReadOnlyCollection<DomainAggregateEvent> events)
    {
        public int Version { get; private set; } = version;
        public IReadOnlyCollection<DomainAggregateEvent> Events { get; private set; } = events;
    }
}
