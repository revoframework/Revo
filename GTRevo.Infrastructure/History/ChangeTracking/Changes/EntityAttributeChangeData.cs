using System;

namespace GTRevo.Infrastructure.History.ChangeTracking.Changes
{
    public abstract class EntityAttributeChangeData : ChangeData
    {
        public string AttributeName { get; set; }
        public abstract Type AttributeType { get; }
    }
}
