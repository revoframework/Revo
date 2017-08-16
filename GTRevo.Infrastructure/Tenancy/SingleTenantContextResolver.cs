using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GTRevo.Infrastructure.Tenancy
{
    public class SingleTenantContextResolver : ITenantContextResolver
    {
        public SingleTenantContextResolver(Guid tenantId)
        {
            TenantId = tenantId;
        }

        public Guid TenantId { get; private set; }

        public Guid? ResolveTenantId(HttpContext httpContext)
        {
            return TenantId;
        }
    }
}
