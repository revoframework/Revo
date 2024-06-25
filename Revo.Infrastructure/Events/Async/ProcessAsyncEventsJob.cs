using System;
using Revo.Infrastructure.Jobs;

namespace Revo.Infrastructure.Events.Async
{
    public class ProcessAsyncEventsJob(string queueName, int attemptsLeft, TimeSpan retryTimeout) : IJob
    {
        public string QueueName { get; } = queueName;
        public int AttemptsLeft { get; } = attemptsLeft;
        public TimeSpan RetryTimeout { get; } = retryTimeout;
    }
}
