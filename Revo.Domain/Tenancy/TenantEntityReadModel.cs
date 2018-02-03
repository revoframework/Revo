using System;
using Revo.Domain.ReadModel;

namespace Revo.Domain.Tenancy
{
    public abstract class TenantEntityReadModel : EntityReadModel, ITenantOwned
    {
        public Guid? TenantId { get; set; }
    }
}
