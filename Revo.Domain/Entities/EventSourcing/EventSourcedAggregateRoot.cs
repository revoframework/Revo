using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Domain.Events;

namespace Revo.Domain.Entities.EventSourcing
{
    /// <summary>
    /// Aggregate root that uses event-sourcing to define its state.
    /// As such, it should only modify its state by publishing events using the protected Publish method.
    /// After publishing an event or loading its state from a repository, the applied events can be observed
    /// and acted upon by implementing private void Apply(EVENT_TYPE) methods (uses convention-based method discovery).
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
            Version += eventCount;
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

        protected sealed override void Publish<T>(T evt)
        {
            base.Publish(evt);
        }
    }
}
