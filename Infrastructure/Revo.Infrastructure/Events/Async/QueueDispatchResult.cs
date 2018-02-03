using System.Collections.Generic;

namespace Revo.Infrastructure.Events.Async
{
    public struct QueueDispatchResult
    {
        public IReadOnlyCollection<IAsyncEventQueueRecord> EnqueuedEventsAsyncProcessed { get; set; }
        public IReadOnlyCollection<IAsyncEventQueueRecord> EnqueuedEventsSyncProcessed { get; set; }
    }
}
