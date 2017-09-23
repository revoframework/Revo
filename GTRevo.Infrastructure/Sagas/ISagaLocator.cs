using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Sagas
{
    public interface ISagaLocator
    {
        Task LocateAndDispatchAsync(IEnumerable<DomainEvent> domainEvents);
    }
}