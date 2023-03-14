using Revo.Core.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public class SingleTenantContextResolver : ITenantContextResolver
    {
        public SingleTenantContextResolver(ITenant tenant)
        {
            Tenant = tenant;
        }

        public ITenant Tenant { get; }

        public ITenant ResolveTenant()
        {
            return Tenant;
        }
    }
}
