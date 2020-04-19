using System.Web;
using Revo.Core.Tenancy;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public class NullTenantContextResolver : ITenantContextResolver
    {
        public ITenant ResolveTenant()
        {
            return null;
        }
    }
}
