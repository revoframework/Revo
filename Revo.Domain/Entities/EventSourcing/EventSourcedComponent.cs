using Revo.Domain.Events;

namespace Revo.Domain.Entities.EventSourcing
{
    public class EventSourcedComponent : IComponent
    {
        public EventSourcedComponent(IAggregateEventRouter eventRouter)
        {
            EventRouter = eventRouter;
            new ConventionEventApplyRegistrator().RegisterEvents(this, EventRouter);
        }

        protected IAggregateEventRouter EventRouter { get; }

        protected virtual void Publish<T>(T evt) where T : DomainAggregateEvent
        {
            EventRouter.Publish(evt);
        }
    }
}
