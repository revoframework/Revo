using System;

namespace Revo.Domain.ReadModel
{
    public class ReadModelForEntityAttribute : Attribute
    {
        public ReadModelForEntityAttribute(Type entityType)
        {
            EntityType = entityType;
        }

        public Type EntityType { get; }
    }
}
