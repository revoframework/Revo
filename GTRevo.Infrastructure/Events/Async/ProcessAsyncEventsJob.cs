using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Jobs;

namespace GTRevo.Infrastructure.Events.Async
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
