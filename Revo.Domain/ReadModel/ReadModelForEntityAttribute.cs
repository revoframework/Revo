using System;

namespace Revo.Domain.ReadModel
{
    public class ReadModelForEntityAttribute(Type entityType) : Attribute
    {
        public Type EntityType { get; } = entityType;
    }
}
