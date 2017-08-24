using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain.Basic;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public abstract class TenantClassifier: BasicClassifier, ITenantOwned
    {
        protected TenantClassifier(Guid id, string code, ITenant tenant) : base(id, code)
        {
            TenantId = tenant?.Id;
        }

        protected TenantClassifier(): base()
        {
        }

        public Guid? TenantId { get; }
    }
}
