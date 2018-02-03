using System;
using System.Web;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public class SingleTenantContextResolver : ITenantContextResolver
    {
        private readonly ITenantManager tenantManager;

        public SingleTenantContextResolver(ITenantManager tenantManager, Guid tenantId)
        {
            this.tenantManager = tenantManager;
            Tenant = tenantManager.GetTenant(tenantId);
        }

        public ITenant Tenant { get; private set; }

        public ITenant ResolveTenant(HttpContext httpContext)
        {
            return Tenant;
        }
    }
}
