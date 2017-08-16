using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GTRevo.Infrastructure.Tenancy
{
    public class NullTenantContextResolver : ITenantContextResolver
    {
        public Guid? ResolveTenantId(HttpContext httpContext)
        {
            return null;
        }
    }
}
