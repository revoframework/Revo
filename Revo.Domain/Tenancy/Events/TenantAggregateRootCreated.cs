using System;
using Revo.Domain.Events;

namespace Revo.Domain.Tenancy.Events
{
    public class TenantAggregateRootCreated : DomainAggregateEvent
    {
        public Guid? TenantId { get; set; }
    }
}
