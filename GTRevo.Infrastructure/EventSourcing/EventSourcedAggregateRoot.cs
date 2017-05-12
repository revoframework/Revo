using System;
using GTRevo.Infrastructure.Domain;
using GTRevo.Infrastructure.Domain.Events;
using GTRevo.Infrastructure.EventSourcing.Events;

namespace GTRevo.Infrastructure.EventSourcing
{
    public abstract class EventSourcedAggregateRoot : AggregateRoot, IEventSourcedAggregateRoot
    {


        public EventSourcedAggregateRoot(Guid id) : base(id)
        {
            new ConventionEventApplyRegistrator().RegisterEvents(this, EventRouter);
            //ApplyEvent(new AggregateCreated());
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

        protected void Delete()
        {
            throw new NotImplementedException();
            //ApplyEvent(new AggregateDeleted());
        }
    }
}
