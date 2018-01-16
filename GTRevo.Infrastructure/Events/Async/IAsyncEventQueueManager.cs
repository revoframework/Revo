using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events.Async
{
    public interface IAsyncEventQueueManager
    {
        Task<IReadOnlyCollection<IAsyncEventQueueRecord>> FindQueuedEventsAsync(Guid[] asyncEventQueueRecordIds);
        Task<IReadOnlyCollection<string>> GetNonemptyQueueNamesAsync();
        Task<IAsyncEventQueueState> GetQueueStateAsync(string queueName);
        Task<IReadOnlyCollection<IAsyncEventQueueRecord>> GetQueueEventsAsync(string queueName);
        Task DequeueEventAsync(Guid asyncEventQueueRecordId);
        Task EnqueueEventAsync(IEventMessage eventMessage, IEnumerable<EventSequencing> queues);
        Task<string> GetEventSourceCheckpointAsync(string eventSourceName);
        Task SetEventSourceCheckpointAsync(string eventSourceName, string opaqueCheckpoint);
        
        /// <returns>New enqueued events.</returns>
        Task<IReadOnlyCollection<IAsyncEventQueueRecord>> CommitAsync();
    }
}
