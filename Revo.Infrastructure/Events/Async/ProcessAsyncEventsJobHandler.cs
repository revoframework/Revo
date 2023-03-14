using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Jobs;
using Revo.Infrastructure.Jobs.InMemory;

namespace Revo.Infrastructure.Events.Async
{
    public class ProcessAsyncEventsJobHandler : IJobHandler<ProcessAsyncEventsJob>
    {
        private readonly IAsyncEventWorker asyncEventWorker;
        private readonly IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration;
        private readonly IInMemoryJobScheduler jobScheduler;
        private readonly ILogger logger;

        public ProcessAsyncEventsJobHandler(IAsyncEventWorker asyncEventWorker,
            IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration,
            IInMemoryJobScheduler jobScheduler, ILogger logger)
        {
            this.asyncEventWorker = asyncEventWorker;
            this.asyncEventPipelineConfiguration = asyncEventPipelineConfiguration;
            this.jobScheduler = jobScheduler;
            this.logger = logger;
        }

        public async Task HandleAsync(ProcessAsyncEventsJob job, CancellationToken cancellationToken)
        {
            logger.LogTrace($"Starting to process async event queue '{job.QueueName}' with {job.AttemptsLeft} attempts left");

            try
            {
                await asyncEventWorker.RunQueueBacklogAsync(job.QueueName);
            }
            catch (AsyncEventProcessingSequenceException e)
            {
                logger.LogDebug(e, $"AsyncEventProcessingSequenceException occurred during asynchronous queue processing");
                await ScheduleRetryAsync(job);
            }
            catch (OptimisticConcurrencyException e)
            {
                logger.LogWarning(e, $"Optimistic concurrency exception occurred while processing '{job.QueueName}' async event queue");
                await ScheduleRetryAsync(job);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception occurred while processing '{job.QueueName}' async event queue");
                throw;
            }

            // TODO handle transient I/O errors with retries
            logger.LogTrace($"Finished processing async event queue '{job.QueueName}'");
        }

        private async Task ScheduleRetryAsync(ProcessAsyncEventsJob job)
        {
            if (job.AttemptsLeft > 1)
            {

                TimeSpan timeout = TimeSpan.FromTicks(job.RetryTimeout.Ticks *
                                                      asyncEventPipelineConfiguration.AsyncProcessRetryTimeoutMultiplier);
                logger.LogDebug($"Scheduling '{job.QueueName}' async event queue processing retry in {timeout.TotalSeconds} seconds");

                ProcessAsyncEventsJob newJob = new ProcessAsyncEventsJob(job.QueueName, job.AttemptsLeft - 1, timeout);
                await jobScheduler.EnqeueJobAsync(newJob, job.RetryTimeout);
            }
            else
            {
                logger.LogError($"Unable to finish '{job.QueueName}' async event queue even after {asyncEventPipelineConfiguration.AsyncProcessAttemptCount} attempts, giving up");
            }
        }
    }
}
