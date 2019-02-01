using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Jobs;
using Revo.Infrastructure.Jobs.InMemory;

namespace Revo.Infrastructure.Events.Async
{
    public class ProcessAsyncEventsJobHandler : IJobHandler<ProcessAsyncEventsJob>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAsyncEventWorker asyncEventWorker;
        private readonly IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration;
        private readonly IInMemoryJobScheduler jobScheduler;

        public ProcessAsyncEventsJobHandler(IAsyncEventWorker asyncEventWorker,
            IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration,
            IInMemoryJobScheduler jobScheduler)
        {
            this.asyncEventWorker = asyncEventWorker;
            this.asyncEventPipelineConfiguration = asyncEventPipelineConfiguration;
            this.jobScheduler = jobScheduler;
        }

        public async Task HandleAsync(ProcessAsyncEventsJob job, CancellationToken cancellationToken)
        {
            Logger.Trace($"Starting to process async event queue '{job.QueueName}' with {job.AttemptsLeft} attempts left");

            try
            {
                await asyncEventWorker.RunQueueBacklogAsync(job.QueueName);
            }
            catch (AsyncEventProcessingSequenceException e)
            {
                Logger.Debug(e, $"AsyncEventProcessingSequenceException occurred during asynchronous queue processing");
                await ScheduleRetryAsync(job);
            }
            catch (OptimisticConcurrencyException e)
            {
                Logger.Warn(e, $"Optimistic concurrency exception occurred while processing '{job.QueueName}' async event queue");
                await ScheduleRetryAsync(job);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Unhandled exception occurred while processing '{job.QueueName}' async event queue");
                throw;
            }

            // TODO handle transient I/O errors with retries
            Logger.Trace($"Finished processing async event queue '{job.QueueName}'");
        }

        private async Task ScheduleRetryAsync(ProcessAsyncEventsJob job)
        {
            if (job.AttemptsLeft > 1)
            {

                TimeSpan timeout = TimeSpan.FromTicks(job.RetryTimeout.Ticks *
                                                      asyncEventPipelineConfiguration.AsyncProcessRetryTimeoutMultiplier);
                Logger.Debug($"Scheduling '{job.QueueName}' async event queue processing retry in {timeout.TotalSeconds} seconds");

                ProcessAsyncEventsJob newJob = new ProcessAsyncEventsJob(job.QueueName, job.AttemptsLeft - 1, timeout);
                await jobScheduler.EnqeueJobAsync(newJob, job.RetryTimeout);
            }
            else
            {
                Logger.Error($"Unable to finish '{job.QueueName}' async event queue even after {asyncEventPipelineConfiguration.AsyncProcessAttemptCount} attempts, giving up");
            }
        }
    }
}
