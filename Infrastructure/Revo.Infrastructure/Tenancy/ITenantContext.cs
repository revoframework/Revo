using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public interface ITenantContext
    {
        ITenant Tenant { get; }
    }
}
