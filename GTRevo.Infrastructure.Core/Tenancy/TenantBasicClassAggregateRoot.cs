using System;
using GTRevo.Infrastructure.Core.Domain.Basic;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public abstract class TenantBasicClassAggregateRoot : BasicClassAggregateRoot, ITenantOwned
    {
        protected TenantBasicClassAggregateRoot(Guid id, ITenant tenant) : base(id)
        {
            TenantId = tenant?.Id;
        }

        protected TenantBasicClassAggregateRoot()
        {
        }

        public Guid? TenantId { get; private set; }
    }
}
