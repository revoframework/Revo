using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Jobs;
using NLog;

namespace GTRevo.Infrastructure.Events.Async
{
    public class ProcessAsyncEventsJobHandler : IJobHandler<ProcessAsyncEventsJob>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAsyncEventQueueBacklogWorker asyncEventQueueBacklogWorker;
        private readonly IJobScheduler jobScheduler;

        public ProcessAsyncEventsJobHandler(IAsyncEventQueueBacklogWorker asyncEventQueueBacklogWorker,
            IJobScheduler jobScheduler)
        {
            this.asyncEventQueueBacklogWorker = asyncEventQueueBacklogWorker;
            this.jobScheduler = jobScheduler;
        }

        public async Task HandleAsync(ProcessAsyncEventsJob job, CancellationToken cancellationToken)
        {
            try
            {
                await asyncEventQueueBacklogWorker.RunQueueBacklogAsync(job.QueueName);
            }
            catch (AsyncEventProcessingSequenceException e)
            {
                Logger.Debug(e, $"AsyncEventProcessingSequenceException occurred during asynchronous queue processing");
                await ScheduleRetryAsync(job);
            }
            catch (OptimisticConcurrencyException e)
            {
                Logger.Debug(e, $"OptimisticConcurrencyException occurred during asynchronous queue processing");
                await ScheduleRetryAsync(job);
            }
        }

        private async Task ScheduleRetryAsync(ProcessAsyncEventsJob job)
        {
            if (job.AttemptsLeft > 1)
            {
                ProcessAsyncEventsJob newJob = new ProcessAsyncEventsJob(job.QueueName, job.AttemptsLeft - 1,
                    TimeSpan.FromTicks(job.RetryTimeout.Ticks * AsyncEventPipelineConfiguration.Current.AsyncProcessRetryTimeoutMultiplier));
                await jobScheduler.EnqeueJobAsync(newJob, job.RetryTimeout);
            }
            else
            {
                Logger.Error($"Not able to finish {job.QueueName} async event queue even with {AsyncEventPipelineConfiguration.Current.AsyncProcessAttemptCount} attempts, giving up");
            }
        }
    }
}
