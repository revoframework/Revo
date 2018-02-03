using System;
using System.Collections.Generic;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public interface ITenantManager
    {
        IEnumerable<ITenant> AllTenants { get; }

        ITenant GetTenant(Guid id);
    }
}
