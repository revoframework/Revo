using System;

namespace Revo.Infrastructure.Jobs.InMemory
{
    public interface IInMemoryJobSchedulerConfiguration
    {
        int HandleAttemptCount { get; }
        int HandleRetryTimeoutMultiplier { get; }
        TimeSpan MaxHandleRetryTimeoutStep { get; }
        TimeSpan MinHandleRetryTimeoutStep { get; }
        int WorkerTaskParallelism { get; }
    }
}
