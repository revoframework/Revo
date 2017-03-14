using GTRevo.Infrastructure.Domain;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcedComponent : IComponent
    {
        public EventSourcedComponent(AggregateEventRouter eventRouter)
        {
            EventRouter = eventRouter;
            new ConventionEventApplyRegistrator().RegisterEvents(this, EventRouter);
        }

        protected AggregateEventRouter EventRouter { get; private set; }

        protected virtual void ApplyEvent<T>(T evt) where T : DomainAggregateEvent
        {
            EventRouter.ApplyEvent(evt);
        }
    }
}
