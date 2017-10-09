using System;
using GTRevo.Infrastructure.Core.Domain.Basic;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public abstract class TenantBasicAggregateRoot : BasicAggregateRoot, ITenantOwned
    {
        public TenantBasicAggregateRoot(Guid id, ITenant tenant) : base(id)
        {
            TenantId = tenant?.Id;
        }

        protected TenantBasicAggregateRoot()
        {
        }

        public Guid? TenantId { get; private set;  }
    }
}
