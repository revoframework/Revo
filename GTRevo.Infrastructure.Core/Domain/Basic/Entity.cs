using System;

namespace GTRevo.Infrastructure.Core.Domain.Basic
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
