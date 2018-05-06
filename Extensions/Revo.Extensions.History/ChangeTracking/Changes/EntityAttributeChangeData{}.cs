using System;

namespace Revo.Extensions.History.ChangeTracking.Changes
{
    public class EntityAttributeChangeData<TAttribute> : EntityAttributeChangeData
    {
        public TAttribute OldValue { get; set; }
        public TAttribute NewValue { get; set; }

        public override Type AttributeType
        {
            get { return typeof(TAttribute); }
        }
    }
}
