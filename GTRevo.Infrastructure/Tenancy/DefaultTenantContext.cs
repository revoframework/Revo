using System;
using System.Web;
using GTRevo.Infrastructure.Core.Tenancy;

namespace GTRevo.Infrastructure.Tenancy
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
