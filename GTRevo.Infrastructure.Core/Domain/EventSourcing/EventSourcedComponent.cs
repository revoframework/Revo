using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Core.Domain.EventSourcing
{
    public class EventSourcedComponent : IComponent
    {
        public EventSourcedComponent(IAggregateEventRouter eventRouter)
        {
            EventRouter = eventRouter;
            new ConventionEventApplyRegistrator().RegisterEvents(this, EventRouter);
        }

        protected IAggregateEventRouter EventRouter { get; }

        protected virtual void ApplyEvent<T>(T evt) where T : DomainAggregateEvent
        {
            EventRouter.ApplyEvent(evt);
        }
    }
}
