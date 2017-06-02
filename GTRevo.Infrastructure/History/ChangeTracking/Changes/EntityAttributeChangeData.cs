using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.History.ChangeTracking.Changes
{
    public abstract class EntityAttributeChangeData : ChangeData
    {
        public string AttributeName { get; set; }
        public abstract Type AttributeType { get; }
    }
}
