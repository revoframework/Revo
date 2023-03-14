using Revo.Core.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    /// <summary>
    /// Resolves the tenant based on the current context (e.g. ambient context, current request, etc.).
    /// </summary>
    public interface ITenantContextResolver
    {
        ITenant ResolveTenant();
    }
}
