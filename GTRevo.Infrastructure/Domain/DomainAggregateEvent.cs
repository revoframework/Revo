using System;

namespace GTRevo.Infrastructure.Domain
{
    public abstract class DomainAggregateEvent : DomainEvent
    {
        public Guid AggregateId { get; set; }
        public Guid AggregateClassId { get; set; }
    }
}
