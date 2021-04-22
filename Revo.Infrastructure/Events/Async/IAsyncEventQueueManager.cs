using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async
{
    public interface IAsyncEventQueueManager
    {
        Task<IReadOnlyCollection<IAsyncEventQueueRecord>> FindQueuedEventsAsync(Guid[] asyncEventQueueRecordIds);
        Task<IReadOnlyCollection<string>> GetNonemptyQueueNamesAsync();
        Task<IAsyncEventQueueState> GetQueueStateAsync(string queueName);
        Task<IReadOnlyCollection<IAsyncEventQueueRecord>> GetQueueEventsAsync(string queueName);
        Task DequeueEventAsync(Guid asyncEventQueueRecordId);
        Task EnqueueEventAsync(IEventMessage eventMessage, IReadOnlyCollection<EventSequencing> queues);
        Task<string> GetEventSourceCheckpointAsync(string eventSourceName);
        Task SetEventSourceCheckpointAsync(string eventSourceName, string opaqueCheckpoint);
        
        /// <returns>New enqueued events.</returns>
        Task<IReadOnlyCollection<IAsyncEventQueueRecord>> CommitAsync();
    }
}
