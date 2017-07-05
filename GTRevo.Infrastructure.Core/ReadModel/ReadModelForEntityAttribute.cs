using System;

namespace GTRevo.Infrastructure.Core.ReadModel
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
