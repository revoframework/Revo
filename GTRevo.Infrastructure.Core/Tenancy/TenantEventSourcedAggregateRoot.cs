using System;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public abstract class TenantEventSourcedAggregateRoot: EventSourcedAggregateRoot, ITenantOwned
    {
        protected TenantEventSourcedAggregateRoot(Guid id, ITenant tenant) : base(id)
        {
            TenantId = tenant?.Id;
        }

        public Guid? TenantId { get; }
    }
}
