using System;
using Revo.Domain.Events;

namespace Revo.Domain.Tenancy.Events
{
    public class TenantAggregateRootCreated : DomainAggregateEvent
    {
        public TenantAggregateRootCreated(Guid? tenantId)
        {
            TenantId = tenantId;
        }

        public Guid? TenantId { get; private set; }
    }
}
