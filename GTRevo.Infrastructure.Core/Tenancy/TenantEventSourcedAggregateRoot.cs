using System;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.Core.Tenancy.Events;

namespace GTRevo.Infrastructure.Core.Tenancy
{
    public abstract class TenantEventSourcedAggregateRoot: EventSourcedAggregateRoot, ITenantOwned
    {
        public TenantEventSourcedAggregateRoot(Guid id, ITenant tenant) : base(id)
        {
            ApplyEvent(new TenantAggregateRootCreated()
            {
                TenantId = tenant?.Id
            });
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
