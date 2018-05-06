using System;

namespace Revo.Extensions.History.ChangeTracking.Changes
{
    public abstract class EntityAttributeChangeData : ChangeData
    {
        public string AttributeName { get; set; }
        public abstract Type AttributeType { get; }
    }
}
