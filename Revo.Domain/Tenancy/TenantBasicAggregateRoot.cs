using System;
using Revo.Core.Tenancy;
using Revo.Domain.Entities.Basic;

namespace Revo.Domain.Tenancy
{
    public abstract class TenantBasicAggregateRoot : BasicAggregateRoot, ITenantOwned
    {
        public TenantBasicAggregateRoot(Guid id, ITenant tenant) : base(id)
        {
            TenantId = tenant?.Id;
        }

        public TenantBasicAggregateRoot(Guid id, Guid? tenantId) : base(id)
        {
            TenantId = tenantId;
        }

        protected TenantBasicAggregateRoot()
        {
        }

        public Guid? TenantId { get; private set;  }
    }
}
