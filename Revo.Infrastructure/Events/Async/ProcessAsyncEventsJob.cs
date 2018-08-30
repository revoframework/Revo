using System;
using Revo.Infrastructure.Jobs;

namespace Revo.Infrastructure.Events.Async
{
    public class ProcessAsyncEventsJob : IJob
    {
        public ProcessAsyncEventsJob(string queueName, int attemptsLeft, TimeSpan retryTimeout)
        {
            QueueName = queueName;
            AttemptsLeft = attemptsLeft;
            RetryTimeout = retryTimeout;
        }

        public string QueueName { get; }
        public int AttemptsLeft { get; }
        public TimeSpan RetryTimeout { get; }
    }
}
