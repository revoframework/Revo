using System;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventPipelineConfiguration : IAsyncEventPipelineConfiguration
    {
        public int CatchUpProcessingParallelism { get; set; } = 80;
        public int SyncQueueProcessingParallelism { get; set; } = 5;

        public int AsyncProcessAttemptCount { get; set; } = 3;
        public TimeSpan AsyncRescheduleDelayAfterSyncProcessFailure { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan AsyncProcessRetryTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
        public int AsyncProcessRetryTimeoutMultiplier { get; set; } = 6;
        public bool WaitForEventCatchUpsUponStartup { get; set; } = false;

        public int SyncProcessAttemptCount { get; set; } = 3;
        public TimeSpan SyncProcessRetryTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
        public int SyncProcessRetryTimeoutMultiplier { get; set; } = 4;
    }
}
