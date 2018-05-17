using System;

namespace Revo.Domain.Tenancy
{
    /// <summary>
    /// A resource (possibly) owned by a tenant.
    /// </summary>
    public interface ITenantOwned
    {
        Guid? TenantId { get; }
    }
}
