using System;
using System.Collections.Generic;
using GTRevo.Infrastructure.Domain.Events;

namespace GTRevo.Infrastructure.Domain
{
    public abstract class AggregateRoot : IAggregateRoot
    {
        public AggregateRoot(Guid id) : this()
        {
            Id = id;
            Version = 0;
        }

        /// <summary>
        /// Only to be used by by ORMs like EF and the second chained constructor.
        /// </summary>
        protected AggregateRoot()
        {
            EventRouter = new AggregateEventRouter(this);
        }

        public virtual Guid Id { get; private set; }
        public virtual int Version { get; protected set; }

        public virtual IEnumerable<DomainAggregateEvent> UncommitedEvents
        {
            get
            {
                return EventRouter.UncommitedEvents;
            }
        }

        protected internal IAggregateEventRouter EventRouter { get; }

        public void Commit()
        {
            Version++;
            EventRouter.CommitEvents();
        }

        protected virtual void ApplyEvent<T>(T evt) where T : DomainAggregateEvent
        {
            EventRouter.ApplyEvent(evt);
        }
    }
}
