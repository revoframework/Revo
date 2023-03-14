using Revo.Core.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    /// <summary>
    /// Provides the tenant for the current context.
    /// The tenant is obtained using a registered ITenantContextResolver (default always returns null tenant).
    /// </summary>
    public interface ITenantContext
    {
        ITenant Tenant { get; }
    }
}
