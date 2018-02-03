using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
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
