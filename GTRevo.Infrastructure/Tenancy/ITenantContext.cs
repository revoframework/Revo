using System;
using GTRevo.Infrastructure.Core.Tenancy;

namespace GTRevo.Infrastructure.Tenancy
{
    public interface ITenantContext
    {
        ITenant Tenant { get; }
    }
}
