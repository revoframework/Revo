using System.Web;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public class DefaultTenantContext : ITenantContext
    {
        public DefaultTenantContext(ITenantContextResolver tenantContextResolver, HttpContext httpContext)
        {
            // resolve immediately to prevent keeping HttpContext alive any longer than needed 
            Tenant = tenantContextResolver.ResolveTenant(httpContext);
        }

        public ITenant Tenant { get; }
    }
}
