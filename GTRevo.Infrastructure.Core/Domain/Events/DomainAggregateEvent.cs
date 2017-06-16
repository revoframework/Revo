using System;

namespace GTRevo.Infrastructure.Domain.Events
{
    public abstract class DomainAggregateEvent : DomainEvent
    {
        public Guid AggregateId { get; set; }
        public Guid AggregateClassId { get; set; }
    }
}
