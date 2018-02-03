using System;

namespace Revo.Infrastructure.Events.Async
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
