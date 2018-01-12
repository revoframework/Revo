using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Events.Async
{
    public struct QueueDispatchResult
    {
        public IReadOnlyCollection<IAsyncEventQueueRecord> EnqueuedEventsAsyncProcessed { get; set; }
        public IReadOnlyCollection<IAsyncEventQueueRecord> EnqueuedEventsSyncProcessed { get; set; }
    }
}
