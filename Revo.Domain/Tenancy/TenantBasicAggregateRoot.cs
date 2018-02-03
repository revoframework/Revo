using System;
using Revo.Domain.Entities.Basic;

namespace Revo.Domain.Tenancy
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
