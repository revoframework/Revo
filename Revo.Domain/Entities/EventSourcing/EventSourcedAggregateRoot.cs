using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Domain.Events;

namespace Revo.Domain.Entities.EventSourcing
{
    /// <summary>
    /// Aggregate root that uses event sourcing to define its state.
    /// As such, it should only modify its state by publishing events using the protected Publish method.
    /// <para>After publishing an event or loading its state from a repository, the applied events can be observed
    /// and acted upon by implementing Apply(EventType) methods with private access modifier and void return type
    /// (uses convention-based method discovery).</para>
    /// </summary>
    public abstract class EventSourcedAggregateRoot : AggregateRoot, IEventSourcedAggregateRoot
    {
        public EventSourcedAggregateRoot(Guid id) : base(id)
        {
            new ConventionEventApplyRegistrator().RegisterEvents(this, EventRouter);
        }

        public override void Commit()
        {
            int eventCount = UncommittedEvents.Count();
            base.Commit();

            if (eventCount > 0)
            {
                Version++;
            }
        }

        public void LoadState(AggregateState state)
        {
            // TODO throw if not at initial state (Version 0 and no uncommittted events?)
            ReplayEvents(state.Events);
            Version = state.Version;
        }

        public void ReplayEvents(IEnumerable<DomainAggregateEvent> events)
        {
            EventRouter.ReplayEvents(events);
        }

        /// <summary>
        /// Publishes a new event.
        /// <para>Upon calling this method, the event gets dispatched to aggregate's event router and
        /// and processed only internally by the components (aggregate root, entities and other components, if registered)
        /// of the aggregate itself (which should not trigger any side effects outside its boundaries).
        /// In event-sourced aggregates, this includes the <c>private void Apply(T)</c> methods automatically
        /// discovered by a convention.</para>
        /// <para>The event does not get delivered to any external listeners until the aggregate gets completely
        /// and fully saved by the repository.</para>
        /// </summary>
        /// <typeparam name="T">Event type.</typeparam>
        /// <param name="evt">Event to publish.</param>
        protected sealed override void Publish<T>(T evt)
        {
            base.Publish(evt);
        }
    }
}
