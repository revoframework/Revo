using Revo.Core.Tenancy;

namespace Revo.Core.Commands
{
    public class CommandTenantContextOverride
    {
        public CommandTenantContextOverride(ITenant tenant)
        {
            Tenant = tenant;
        }

        public ITenant Tenant { get; }
    }
}