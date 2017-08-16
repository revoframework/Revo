using System;

namespace GTRevo.Infrastructure.Tenancy
{
    public interface ITenantContext
    {
        Guid? TenantId { get; }
    }
}
