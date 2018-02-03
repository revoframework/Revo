using System;

namespace Revo.Domain.Entities.Basic
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

        public override string ToString()
        {
            return $"{GetType().FullName} (ID: {Id})";
        }
    }
}
