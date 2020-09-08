using System;
using Revo.Domain.ReadModel;

namespace Revo.Domain.Tenancy
{
    public abstract class TenantReadModelBase : ReadModelBase, ITenantOwned
    {
        public Guid? TenantId { get; set; }
    }
}