using System;
using GTRevo.Infrastructure.Domain;

namespace GTRevo.Infrastructure.EventSourcing
{
    public abstract class EventSourcedAggregateRoot : AggregateRoot, IEventSourcedAggregateRoot
    {
        public EventSourcedAggregateRoot(Guid id, Guid classId) : base(id, classId)
        {
            new ConventionEventApplyRegistrator().RegisterEvents(this, EventRouter);
        }

        public void LoadState(AggregateState state)
        {
            EventRouter.ReplayEvents(state.Events);
            Version = state.Version;
        }

        protected override void ApplyEvent<T>(T evt)
        {
            base.ApplyEvent(evt);
            EventRouter.ApplyEvent(evt);
        }
    }
}
