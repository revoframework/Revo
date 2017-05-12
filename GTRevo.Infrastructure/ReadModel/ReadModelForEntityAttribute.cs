using System;

namespace GTRevo.Infrastructure.ReadModel
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
