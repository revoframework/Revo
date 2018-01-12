using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Events.Async
{
    public interface IAsyncEventProcessor
    {
        Task EnqueueForAsyncProcessing(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess, TimeSpan? timeDelay, TimeSpan retryTimeout);
        Task ProcessSynchronously(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess);
    }
}