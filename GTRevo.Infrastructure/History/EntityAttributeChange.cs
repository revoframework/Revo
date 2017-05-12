using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.History
{
    public class EntityAttributeChange<TAttribute>
    {
        public string AttributeName { get; set; }
        public TAttribute InitialValue { get; set; }
        public TAttribute NewValue { get; set; }
    }
}
