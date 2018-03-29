using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Sagas
{
    public interface ISagaEventDispatcher
    {
        Task DispatchEventsToSagas(IEnumerable<IEventMessage<DomainEvent>> eventMessages);
    }
}