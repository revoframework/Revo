using System;
using System.Web;

namespace GTRevo.Infrastructure.Tenancy
{
    public class DefaultTenantContext : ITenantContext
    {
        private readonly ITenantContextResolver tenantContextResolver;
        private readonly HttpContext httpContext;
        private readonly Lazy<Guid?> tenantIdLazy;

        public DefaultTenantContext(ITenantContextResolver tenantContextResolver, HttpContext httpContext)
        {
            this.tenantContextResolver = tenantContextResolver;
            this.httpContext = httpContext;
            this.tenantIdLazy = new Lazy<Guid?>(() =>
            {
                return tenantContextResolver.ResolveTenantId(httpContext);
            });
        }

        public Guid? TenantId => tenantIdLazy.Value;
    }
}
