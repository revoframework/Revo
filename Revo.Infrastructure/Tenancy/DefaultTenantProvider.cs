using System;
using Revo.Core.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public class DefaultTenantProvider : ITenantProvider
    {
        public ITenant GetTenant(Guid? id)
        {
            if (id == null)
            {
                return null;
            }

            throw new InvalidOperationException(
                "DefaultTenantProvider.GetTenant is a null implementation and cannot return any tenants");
        }
    }
}
