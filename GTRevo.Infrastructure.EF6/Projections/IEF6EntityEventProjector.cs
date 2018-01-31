using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.Projections;

namespace GTRevo.Infrastructure.EF6.Projections
{
    public interface IEF6EntityEventProjector<T> : IEntityEventProjector<T>
        where T : IEventSourcedAggregateRoot
    {
    }
}
