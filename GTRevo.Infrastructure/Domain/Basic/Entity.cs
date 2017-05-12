using System;

namespace GTRevo.Infrastructure.Domain.Basic
{
    public abstract class Entity : IEntity
    {
        public Entity(Guid id)
        {
            Id = id;
        }

        protected Entity()
        {
        }

        public Guid Id { get; private set; }
    }
}
