using System;
using Revo.Domain.Entities.Basic;

namespace Revo.Domain.Tenancy
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
