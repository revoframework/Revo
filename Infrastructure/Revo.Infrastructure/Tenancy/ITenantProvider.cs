using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Tenancy
{
    public interface ITenantProvider
    {
        ITenant GetTenant(Guid id);
    }
}
