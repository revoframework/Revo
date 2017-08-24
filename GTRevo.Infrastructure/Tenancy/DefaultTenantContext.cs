using System;
using System.Web;
using GTRevo.Infrastructure.Core.Tenancy;

namespace GTRevo.Infrastructure.Tenancy
{
    public class DefaultTenantContext : ITenantContext
    {
        private readonly ITenantContextResolver tenantContextResolver;
        private readonly HttpContext httpContext;
        private readonly Lazy<ITenant> tenantIdLazy;

        public DefaultTenantContext(ITenantContextResolver tenantContextResolver, HttpContext httpContext)
        {
            this.tenantContextResolver = tenantContextResolver;
            this.httpContext = httpContext;
            this.tenantIdLazy = new Lazy<ITenant>(() =>
            {
                return tenantContextResolver.ResolveTenant(httpContext);
            });
        }

        public ITenant Tenant => tenantIdLazy.Value;
    }
}
