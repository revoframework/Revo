﻿using Revo.Core.Core;
using Revo.Core.Tenancy;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public class DefaultTenantContext : ITenantContext
    {
        public DefaultTenantContext(ITenantContextResolver tenantContextResolver)
        {
            // resolve immediately
            var tenantOverride = TenantContextOverride.Current;
            if (tenantOverride != null)
            {
                Tenant = tenantOverride.Tenant;
            }
            else
            {
                Tenant = tenantContextResolver.ResolveTenant();
            }
        }

        public ITenant Tenant { get; }
    }
}
