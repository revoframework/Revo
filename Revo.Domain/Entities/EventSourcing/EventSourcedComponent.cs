using Revo.Domain.Events;

namespace Revo.Domain.Entities.EventSourcing
{
    /// <summary>
    /// Event-sourced aggregate component that publishes events and reacts
    /// to other events published inside the aggregate.
    /// As such, it should only modify its state by publishing events using the EventRouter.
    /// <para>Uses convention-based discovery of event Apply(EventType) methods.</para>
    /// </summary>
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
