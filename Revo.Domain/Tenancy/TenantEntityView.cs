using System;
using Revo.Domain.ReadModel;

namespace Revo.Domain.Tenancy
{
    public abstract class TenantEntityView : EntityView, ITenantOwned
    {
        public Guid? TenantId { get; set; }
    }
}
