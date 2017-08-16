using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.ReadModel;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public class TenantEntityReadModel : EntityReadModel, ITenantOwned
    {
        public Guid? TenantId { get; set; }
    }
}
