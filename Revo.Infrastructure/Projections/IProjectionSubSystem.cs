using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Projections
{
    public interface IProjectionSubSystem
    {
        Task ExecuteProjectionsAsync(IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events,
            IUnitOfWork unitOfWork, EventProjectionOptions options);
        //Task ReplayProjectionsAsync...
    }
}
