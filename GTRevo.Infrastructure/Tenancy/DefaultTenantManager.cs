using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Tenancy;

namespace GTRevo.Infrastructure.Tenancy
{
    public class DefaultTenantManager : ITenantManager
    {
        public IEnumerable<ITenant> AllTenants => Enumerable.Empty<ITenant>();

        public ITenant GetTenant(Guid id)
        {
            throw new InvalidOperationException(
                "DefaultTenantManager.GetTenant is a null implementation and cannot return any tenants");
        }
    }
}
