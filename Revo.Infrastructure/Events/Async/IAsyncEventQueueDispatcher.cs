using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async
{
    public interface IAsyncEventQueueDispatcher
    {
        Task<QueueDispatchResult> DispatchToQueuesAsync(IEnumerable<IEventMessage> eventMessages,
            string eventSourceName, string eventSourceCheckpoint);
        Task<string> GetLastEventSourceDispatchCheckpointAsync(string eventSourceName);
    }
}