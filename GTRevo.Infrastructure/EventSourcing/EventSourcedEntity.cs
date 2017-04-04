using System;
using GTRevo.Infrastructure.Domain;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcedEntity : EventSourcedComponent, IEntityBase
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
            IEntityBase other = obj as IEntityBase;
            return Id == other?.Id;
        }
    }
}
