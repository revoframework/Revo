using Revo.Core.Tenancy;

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
