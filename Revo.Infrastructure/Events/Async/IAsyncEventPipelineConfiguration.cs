using System;

namespace Revo.Infrastructure.Events.Async
{
    /// <summary>
    /// Defines the configuration of the event processing pipeline.
    /// </summary>
    public interface IAsyncEventPipelineConfiguration
    {
        /// <summary>
        /// Number of Tasks to be run in parallel when doing event execution catch-ups upon the applications starts.
        /// Does not directly influence the amount of threads spawned, only limits the number of unfinished processing tasks at a time.
        /// </summary>
        int CatchUpProcessingParallelism { get; }

        /// <summary>
        /// Number of Tasks to be run in parallel (per request) when synchronously processing event queues after a request.
        /// Does not directly influence the amount of threads spawned, only limits the number of unfinished processing tasks at a time.
        /// </summary>
        int SyncQueueProcessingParallelism { get; }

        /// <summary>
        /// Maximum total number of Tasks to be run in parallel by the event daemon when asynchronously processing event queues.
        /// Does not directly influence the amount of threads spawned, only limits the number of unfinished processing tasks at a time.
        /// </summary>
        //int AsyncQueueProcessingParallelism { get; }

        int AsyncProcessAttemptCount { get; }
        TimeSpan AsyncRescheduleDelayAfterSyncProcessFailure { get; }
        TimeSpan AsyncProcessRetryTimeout { get; }
        int AsyncProcessRetryTimeoutMultiplier { get; }
        bool WaitForEventCatchUpsUponStartup { get; }

        int SyncProcessAttemptCount { get; }
        TimeSpan SyncProcessRetryTimeout { get; }
        int SyncProcessRetryTimeoutMultiplier { get; }
    }
}
