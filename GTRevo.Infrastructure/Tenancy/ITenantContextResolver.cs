using System;
using System.Web;

namespace GTRevo.Infrastructure.Tenancy
{
    public interface ITenantContextResolver
    {
        Guid? ResolveTenantId(HttpContext httpContext);
    }
}
