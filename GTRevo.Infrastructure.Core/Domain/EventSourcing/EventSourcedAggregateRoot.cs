using System;
using System.Collections.Generic;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Core.Domain.EventSourcing
{
    public abstract class EventSourcedAggregateRoot : AggregateRoot, IEventSourcedAggregateRoot
    {
        public EventSourcedAggregateRoot(Guid id) : base(id)
        {
            new ConventionEventApplyRegistrator().RegisterEvents(this, EventRouter);
            //ApplyEvent(new AggregateCreated());
        }

        public override void Commit()
        {
            base.Commit();
            Version++;
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

        protected override void ApplyEvent<T>(T evt)
        {
            base.ApplyEvent(evt);
        }
    }
}
