using System;

namespace Revo.Infrastructure.Jobs.InMemory
{
    public class InMemoryJobSchedulerConfiguration : IInMemoryJobSchedulerConfiguration
    {
        public int WorkerTaskParallelism { get; set; } = 10;
        public int HandleAttemptCount { get; set; } = 3;
        public TimeSpan MinHandleRetryTimeoutStep { get; set; } = TimeSpan.FromMilliseconds(100);
        public TimeSpan MaxHandleRetryTimeoutStep { get; set; } = TimeSpan.FromMilliseconds(500);
        public int HandleRetryTimeoutMultiplier { get; set; } = 5;
    }
}
