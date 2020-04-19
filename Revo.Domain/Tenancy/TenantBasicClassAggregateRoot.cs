using System;
using Revo.Core.Tenancy;
using Revo.Domain.Entities.Basic;

namespace Revo.Domain.Tenancy
{
    public abstract class TenantBasicClassAggregateRoot : BasicClassAggregateRoot, ITenantOwned
    {
        public TenantBasicClassAggregateRoot(Guid id, ITenant tenant) : base(id)
        {
            TenantId = tenant?.Id;
        }

        public TenantBasicClassAggregateRoot(Guid id, Guid? tenantId) : base(id)
        {
            TenantId = tenantId;
        }

        protected TenantBasicClassAggregateRoot()
        {
        }

        public Guid? TenantId { get; private set; }
    }
}
