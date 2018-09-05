using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Domain.Entities.Basic;
using Revo.Domain.Events;

namespace Revo.Domain.Entities
{
    /// <summary>
    /// Abstract base class for aggregate root implementations.
    /// Implements a basic aggregate-event-routing and convention-based discovery of Apply methods
    /// that can act upon the events published inside the aggregate.
    /// </summary>
    public abstract class AggregateRoot : Entity, IAggregateRoot
    {
        public AggregateRoot(Guid id) : base(id)
        {
            EventRouter = new AggregateEventRouter(this);
        }

        /// <summary>
        /// Only to be used by by ORMs like EF and the second chained constructor.
        /// </summary>
        protected AggregateRoot()
        {
            EventRouter = new AggregateEventRouter(this);
        }

        public bool IsDeleted { get; private set; }
        public virtual int Version { get; protected set; } = 0;

        public virtual bool IsChanged => UncommittedEvents.Any();
        public virtual IReadOnlyCollection<DomainAggregateEvent> UncommittedEvents => EventRouter.UncommitedEvents;

        protected internal IAggregateEventRouter EventRouter { get; }

        public virtual void Commit()
        {
            EventRouter.CommitEvents();
        }

        public override string ToString()
        {
            return $"{GetType().Name} {{ Id = {Id} }}";
        }

        /// <summary>
        /// Publishes a new event.
        /// <para>Upon calling this method, the event gets dispatched to aggregate's event router and
        /// and processed only internally by the components (aggregate root, entities and other components, if registered)
        /// of the aggregate itself (which should not trigger any side effects outside its boundaries).
        /// The event does not get delivered to any external listeners until the aggregate gets completely
        /// and fully saved by the repository.</para>
        /// </summary>
        /// <typeparam name="T">Event type.</typeparam>
        /// <param name="evt">Event to publish.</param>
        protected virtual void Publish<T>(T evt) where T : DomainAggregateEvent
        {
            if (IsDeleted)
            {
                throw new InvalidOperationException(
                    $"Cannot publish new {typeof(T).FullName} event on {this} aggregate because it is currently in deleted state");
            }

            EventRouter.Publish(evt);
        }

        protected void MarkDeleted()
        {
            IsDeleted = true;
        }
    }
}
