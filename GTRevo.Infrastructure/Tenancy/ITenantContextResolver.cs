using System;
using System.Web;
using GTRevo.Infrastructure.Core.Tenancy;

namespace GTRevo.Infrastructure.Tenancy
{
    public interface ITenantContextResolver
    {
        ITenant ResolveTenant(HttpContext httpContext);
    }
}
