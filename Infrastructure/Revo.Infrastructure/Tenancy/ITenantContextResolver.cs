using System.Web;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public interface ITenantContextResolver
    {
        ITenant ResolveTenant(HttpContext httpContext);
    }
}
