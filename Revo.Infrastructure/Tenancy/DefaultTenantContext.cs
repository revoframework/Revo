using System.Web;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public class DefaultTenantContext : ITenantContext
    {
        public DefaultTenantContext(ITenantContextResolver tenantContextResolver)
        {
            // resolve immediately to prevent keeping HttpContext alive any longer than needed 
            Tenant = tenantContextResolver.ResolveTenant();
        }

        public ITenant Tenant { get; }
    }
}
