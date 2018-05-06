using System;
using System.Web;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public class SingleTenantContextResolver : ITenantContextResolver
    {
        private readonly ITenantProvider tenantProvider;

        public SingleTenantContextResolver(ITenantProvider tenantProvider, Guid tenantId)
        {
            this.tenantProvider = tenantProvider;
            Tenant = tenantProvider.GetTenant(tenantId);
        }

        public ITenant Tenant { get; private set; }

        public ITenant ResolveTenant()
        {
            return Tenant;
        }
    }
}
