using Revo.Core.Tenancy;

namespace Revo.Core.Commands
{
    public class CommandTenantContextOverride(ITenant tenant)
    {
        public ITenant Tenant { get; } = tenant;
    }
}