using System;
using GTRevo.Infrastructure.Core.Domain.Basic;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public class TenantBasicClassAggregateRoot : BasicClassAggregateRoot, ITenantOwned
    {
        protected TenantBasicClassAggregateRoot(Guid id, Guid? tenantId) : base(id)
        {
            TenantId = tenantId;
        }

        protected TenantBasicClassAggregateRoot()
        {
        }

        public Guid? TenantId { get; }
    }
}
