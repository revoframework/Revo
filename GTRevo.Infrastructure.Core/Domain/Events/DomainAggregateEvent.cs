using System;

namespace GTRevo.Infrastructure.Core.Domain.Events
{
    public abstract class DomainAggregateEvent : DomainEvent
    {
        public Guid AggregateId { get; set; }
    }
}
