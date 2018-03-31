using System;
using Revo.Domain.ReadModel;

namespace Revo.Domain.Tenancy
{
    public abstract class TenantEntityReadModel : EntityReadModel, ITenantOwned, ITenantReadModel
    {
        public Guid? TenantId { get; set; }
    }
}
