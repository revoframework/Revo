using System;

namespace Revo.Domain.Tenancy
{
    public interface ITenantOwned
    {
        Guid? TenantId { get; }
    }
}
