using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GTRevo.Infrastructure.Core.Tenancy;

namespace GTRevo.Infrastructure.Tenancy
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
