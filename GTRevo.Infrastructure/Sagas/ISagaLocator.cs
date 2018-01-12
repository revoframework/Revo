using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Sagas
{
    public interface ISagaLocator
    {
        Task LocateAndDispatchAsync(IEnumerable<IEventMessage<DomainEvent>> domainEvents);
    }
}