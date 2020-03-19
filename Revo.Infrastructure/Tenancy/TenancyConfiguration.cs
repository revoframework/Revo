namespace Revo.Infrastructure.Tenancy
{
    public class TenancyConfiguration
    {
        public bool UseNullTenantContextResolver { get; set; } = true;
        public bool UseDefaultTenantProvider { get; set; } = true;
        public bool EnableTenantRepositoryFilter { get; set; } = true;
        public bool NullTenantCanAccessOtherTenantsData { get; set; } = false;
    }
}