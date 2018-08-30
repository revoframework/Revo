using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public class DefaultTenantProvider : ITenantProvider
    {
        public ITenant GetTenant(Guid id)
        {
            throw new InvalidOperationException(
                "DefaultTenantProvider.GetTenant is a null implementation and cannot return any tenants");
        }
    }
}
