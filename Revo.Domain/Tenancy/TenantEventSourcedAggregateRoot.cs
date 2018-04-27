using System;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Tenancy.Events;

namespace Revo.Domain.Tenancy
{
    public abstract class TenantEventSourcedAggregateRoot: EventSourcedAggregateRoot, ITenantOwned
    {
        public TenantEventSourcedAggregateRoot(Guid id, ITenant tenant) : base(id)
        {
            Publish(new TenantAggregateRootCreated(tenant?.Id));
        }

        protected TenantEventSourcedAggregateRoot(Guid id) : base(id)
        {
        }

        public Guid? TenantId { get; private set; }

        private void Apply(TenantAggregateRootCreated ev)
        {
            TenantId = ev.TenantId;
        }
    }
}
