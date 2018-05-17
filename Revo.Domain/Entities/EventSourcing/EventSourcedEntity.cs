using System;
using Revo.Domain.Events;

namespace Revo.Domain.Entities.EventSourcing
{
    /// <summary>
    /// Event-sourced aggregate entity.
    /// As such, it should only modify its state by publishing events using the EventRouter.
    /// <para>After publishing an event or loading its state from a repository, the applied events can be observed
    /// and acted upon by implementing Apply(EventType) methods with private access modifier and void return type
    /// (uses convention-based method discovery).</para>
    /// </summary>
    public class EventSourcedEntity : EventSourcedComponent, IEntity
    {
        public EventSourcedEntity(Guid id, IAggregateEventRouter eventRouter) : base(eventRouter)
        {
            Id = id;
        }

        public Guid Id { get; }
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            IEntity other = obj as IEntity;
            return Id == other?.Id;
        }
        
        public override string ToString()
        {
            return $"{GetType().Name} {{ Id = {Id} }}";
        }
    }
}
