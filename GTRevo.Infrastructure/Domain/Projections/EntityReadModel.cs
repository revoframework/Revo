using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Domain.Projections
{
    public abstract class EntityReadModel
    {
        public Guid Id { get; set; }
    }
}
