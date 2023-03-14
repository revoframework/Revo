using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Jobs.InMemory;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventProcessor : IAsyncEventProcessor
    {
        private readonly Func<IAsyncEventWorker> asyncEventWorkerFunc;
        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly IInMemoryJobScheduler jobScheduler;
        private readonly IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration;
        private readonly ILogger logger;

        public AsyncEventProcessor(Func<IAsyncEventWorker> asyncEventWorkerFunc,
            IAsyncEventQueueManager asyncEventQueueManager,
            IInMemoryJobScheduler jobScheduler,
            IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration,
            ILogger logger)
        {
            this.asyncEventWorkerFunc = asyncEventWorkerFunc;
            this.asyncEventQueueManager = asyncEventQueueManager;
            this.jobScheduler = jobScheduler;
            this.asyncEventPipelineConfiguration = asyncEventPipelineConfiguration;
            this.logger = logger;
        }

        public async Task ProcessSynchronously(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess)
        {
            string[] queues = eventsToProcess.Select(x => x.QueueName).Distinct().ToArray();
            List<IAsyncEventQueueRecord> remainingEvents = eventsToProcess.ToList();

            int triesLeft = asyncEventPipelineConfiguration.SyncProcessAttemptCount;
            TimeSpan sleepTime = asyncEventPipelineConfiguration.SyncProcessRetryTimeout;
            while (triesLeft > 0)
            {
                triesLeft--;
                queues = await TryRunQueues(queues);
                if (queues.Length == 0)
                {
                    break;
                }

                remainingEvents = (await asyncEventQueueManager.FindQueuedEventsAsync(remainingEvents.Select(x => x.Id).ToArray())).ToList();
                if (remainingEvents.Count == 0)
                {
                    break;
                }

                if (triesLeft > 0)
                {
                    await Sleep.Current.SleepAsync(sleepTime);
                    sleepTime = TimeSpan.FromTicks(sleepTime.Ticks * asyncEventPipelineConfiguration.SyncProcessRetryTimeoutMultiplier);
                }
            }

            if (queues.Length > 0 && remainingEvents.Count > 0)
            {
                logger.LogError(
                    $"Not able to synchronously process all event queues, about to reschedule {remainingEvents.Count} events for later async processing in {asyncEventPipelineConfiguration.AsyncRescheduleDelayAfterSyncProcessFailure.TotalSeconds:0.##} seconds");
                await EnqueueForAsyncProcessingAsync(remainingEvents, asyncEventPipelineConfiguration.AsyncRescheduleDelayAfterSyncProcessFailure);
            }
        }

        public async Task EnqueueForAsyncProcessingAsync(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess, TimeSpan? timeDelay)
        {
            string[] queues = eventsToProcess.Select(x => x.QueueName).Distinct().ToArray();
            var jobs = queues.Select(x => new ProcessAsyncEventsJob(x, asyncEventPipelineConfiguration.AsyncProcessAttemptCount, asyncEventPipelineConfiguration.AsyncProcessRetryTimeout));

            foreach (ProcessAsyncEventsJob job in jobs)
            {
                await jobScheduler.EnqeueJobAsync(job, timeDelay);
            }
        }

        private async Task<string[]> TryRunQueues(string[] queues)
        {
            var throttledTasks = new List<Task<string>>();

            using (var throttle = new SemaphoreSlim(asyncEventPipelineConfiguration.SyncQueueProcessingParallelism))
            {
                foreach (string queueName in queues)
                {
                    await throttle.WaitAsync();

                    throttledTasks.Add(Task.Factory.StartNewWithContext(async () =>
                    {
                        try
                        {
                            var asyncEventQueueBacklogWorker = asyncEventWorkerFunc();
                            await asyncEventQueueBacklogWorker.RunQueueBacklogAsync(queueName);
                            return queueName;
                        }
                        catch (AsyncEventProcessingSequenceException e)
                        {
                            logger.LogDebug(e,
                                $"AsyncEventProcessingSequenceException occurred during synchronous queue processing");
                            return null; //can retry
                        }
                        catch (OptimisticConcurrencyException e)
                        {
                            logger.LogDebug(e,
                                $"OptimisticConcurrencyException occurred during synchronous queue processing");
                            return null; //can retry
                        }
                        finally
                        {
                            throttle.Release();
                        }
                    }));
                }

                string[] finishedQueues = await Task.WhenAll(throttledTasks);
                return queues.Except(finishedQueues).ToArray();
            }
        }
    }
}
