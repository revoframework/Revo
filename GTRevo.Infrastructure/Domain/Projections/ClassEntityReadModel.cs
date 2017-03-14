using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Domain.Projections
{
    public abstract class ClassEntityReadModel : EntityReadModel
    {
        public Guid ClassId { get; set; }
    }
}
