using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.ReadModel;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public abstract class TenantEntityView : EntityView, ITenantOwned
    {
        public Guid? TenantId { get; set; }
    }
}
