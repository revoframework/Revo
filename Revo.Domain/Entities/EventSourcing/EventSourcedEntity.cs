using System;
using Revo.Domain.Events;

namespace Revo.Domain.Entities.EventSourcing
{
    public class EventSourcedEntity : EventSourcedComponent, IEntity
    {
        public EventSourcedEntity(Guid id, IAggregateEventRouter eventRouter) : base(eventRouter)
        {
            Id = id;
        }

        public Guid Id { get; private set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            IEntity other = obj as IEntity;
            return Id == other?.Id;
        }
    }
}
