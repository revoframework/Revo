using System;

namespace Revo.Domain.Events
{
    public abstract class DomainAggregateEvent : DomainEvent
    {
        public Guid AggregateId { get; set; }
    }
}
