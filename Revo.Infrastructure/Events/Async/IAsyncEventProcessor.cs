using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Events.Async
{
    public interface IAsyncEventProcessor
    {
        Task EnqueueForAsyncProcessingAsync(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess, TimeSpan? timeDelay);
        Task ProcessSynchronously(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess);
    }
}