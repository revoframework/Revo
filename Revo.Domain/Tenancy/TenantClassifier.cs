using System;
using Revo.Domain.Entities.Basic;

namespace Revo.Domain.Tenancy
{
    public abstract class TenantClassifier: BasicClassifier, ITenantOwned
    {
        protected TenantClassifier(Guid id, string code, ITenant tenant) : base(id, code)
        {
            TenantId = tenant?.Id;
        }

        protected TenantClassifier()
        {
        }

        public Guid? TenantId { get; private set; }
    }
}
