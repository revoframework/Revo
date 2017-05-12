using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.ReadModel
{
    public abstract class ClassEntityView : EntityView
    {
        public Guid ClassId { get; set; }
    }
}
