using System;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public interface ITenantOwned
    {
        Guid? TenantId { get; }
    }
}
