using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Tenancy;

namespace GTRevo.Infrastructure.Tenancy
{
    public interface ITenantManager
    {
        IEnumerable<ITenant> AllTenants { get; }

        ITenant GetTenant(Guid id);
    }
}
