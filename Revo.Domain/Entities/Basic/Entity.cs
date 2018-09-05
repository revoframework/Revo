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

        public virtual Guid Id { get; private set; }

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
