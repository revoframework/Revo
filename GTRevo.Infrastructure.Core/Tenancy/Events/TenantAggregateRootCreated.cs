using System;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Core.Tenancy.Events
{
    public class TenantAggregateRootCreated : DomainAggregateEvent
    {
        public Guid? TenantId { get; set; }
    }
}
