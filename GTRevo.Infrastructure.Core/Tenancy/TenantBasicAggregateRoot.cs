using System;
using GTRevo.Infrastructure.Core.Domain.Basic;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public abstract class TenantBasicAggregateRoot : BasicAggregateRoot, ITenantOwned
    {
        protected TenantBasicAggregateRoot(Guid id, Guid? tenantId) : base(id)
        {
            TenantId = tenantId;
        }

        protected TenantBasicAggregateRoot()
        {
        }

        public Guid? TenantId { get; }
    }
}
