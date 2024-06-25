using System;
using Revo.Domain.Events;

namespace Revo.Domain.Tenancy.Events
{
    public class TenantAggregateRootCreated(Guid? tenantId) : DomainAggregateEvent
    {
        public Guid? TenantId { get; private set; } = tenantId;
    }
}
