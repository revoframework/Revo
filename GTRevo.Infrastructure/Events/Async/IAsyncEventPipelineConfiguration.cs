using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Events.Async
{
    public interface IAsyncEventPipelineConfiguration
    {
        int AsyncProcessAttemptCount { get; }
        TimeSpan AsyncRescheduleDelayAfterSyncProcessFailure { get; }
        TimeSpan AsyncProcessRetryTimeout { get; }
        int AsyncProcessRetryTimeoutMultiplier { get; }

        int SyncProcessAttemptCount { get; }
        TimeSpan SyncProcessRetryTimeout { get; }
        int SyncProcessRetryTimeoutMultiplier { get; }
    }
}
