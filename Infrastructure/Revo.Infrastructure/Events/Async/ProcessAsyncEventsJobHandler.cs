using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Jobs;

namespace Revo.Infrastructure.Events.Async
{
    public class ProcessAsyncEventsJobHandler : IJobHandler<ProcessAsyncEventsJob>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAsyncEventQueueBacklogWorker asyncEventQueueBacklogWorker;
        private readonly IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration;
        private readonly IJobScheduler jobScheduler;

        public ProcessAsyncEventsJobHandler(IAsyncEventQueueBacklogWorker asyncEventQueueBacklogWorker,
            IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration,
            IJobScheduler jobScheduler)
        {
            this.asyncEventQueueBacklogWorker = asyncEventQueueBacklogWorker;
            this.asyncEventPipelineConfiguration = asyncEventPipelineConfiguration;
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
                    TimeSpan.FromTicks(job.RetryTimeout.Ticks * asyncEventPipelineConfiguration.AsyncProcessRetryTimeoutMultiplier));
                await jobScheduler.EnqeueJobAsync(newJob, job.RetryTimeout);
            }
            else
            {
                Logger.Error($"Not able to finish {job.QueueName} async event queue even with {asyncEventPipelineConfiguration.AsyncProcessAttemptCount} attempts, giving up");
            }
        }
    }
}
