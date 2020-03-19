using System;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public interface ITenantProvider
    {
        ITenant GetTenant(Guid? id);
    }
}
