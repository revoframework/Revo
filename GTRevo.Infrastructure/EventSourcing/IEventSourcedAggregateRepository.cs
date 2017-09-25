using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;

namespace GTRevo.Infrastructure.EventSourcing
{
    public interface IEventSourcedAggregateRepository : IEventSourcedRepository<IEventSourcedAggregateRoot>
    {
    }
}
