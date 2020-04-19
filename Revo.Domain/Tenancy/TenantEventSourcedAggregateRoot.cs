using System;
using Revo.Core.Tenancy;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Tenancy.Events;

namespace Revo.Domain.Tenancy
{
    /// <summary>
    /// Event-sourced aggregate root that is owned by a tenant.
    /// <para>Upon its creation, publishes a new TenantAggregateRootCreated that specified the tenant it belongs to.</para>
    /// <para><see cref="EventSourcedAggregateRoot"/></para>
    /// </summary>
    public abstract class TenantEventSourcedAggregateRoot : EventSourcedAggregateRoot, ITenantOwned
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
