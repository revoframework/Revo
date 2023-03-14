using System;
using Revo.Core.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public interface ITenantProvider
    {
        ITenant GetTenant(Guid? id);
    }
}
