using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Jobs;
using MoreLinq;
using NLog;

namespace GTRevo.Infrastructure.Events.Async
{
    public class AsyncEventProcessor : IAsyncEventProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<IAsyncEventQueueBacklogWorker> asyncEventQueueBacklogWorkerFunc;
        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly IJobScheduler jobScheduler;

        public AsyncEventProcessor(Func<IAsyncEventQueueBacklogWorker> asyncEventQueueBacklogWorkerFunc,
            IAsyncEventQueueManager asyncEventQueueManager,
            IJobScheduler jobScheduler)
        {
            this.asyncEventQueueBacklogWorkerFunc = asyncEventQueueBacklogWorkerFunc;
            this.asyncEventQueueManager = asyncEventQueueManager;
            this.jobScheduler = jobScheduler;
        }

        public async Task ProcessSynchronously(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess)
        {
            string[] queues = eventsToProcess.Select(x => x.QueueName).Distinct().ToArray();
            List<IAsyncEventQueueRecord> remainingEvents = eventsToProcess.ToList();

            int triesLeft = AsyncEventPipelineConfiguration.Current.SyncProcessAttemptCount;
            TimeSpan sleepTime = AsyncEventPipelineConfiguration.Current.SyncProcessRetryTimeout;
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
                    sleepTime = TimeSpan.FromTicks(sleepTime.Ticks * AsyncEventPipelineConfiguration.Current.SyncProcessRetryTimeoutMultiplier);
                }
            }

            if (queues.Length > 0 && remainingEvents.Count > 0)
            {
                Logger.Error(
                    $"Not able to synchronously process all event queues, about to reschedule {remainingEvents.Count} events for later async processing in {AsyncEventPipelineConfiguration.Current.AsyncRescheduleDelayAfterSyncProcessFailure.TotalSeconds:0.##} seconds");
                await EnqueueForAsyncProcessingAsync(remainingEvents, AsyncEventPipelineConfiguration.Current.AsyncRescheduleDelayAfterSyncProcessFailure);
            }
        }

        public async Task EnqueueForAsyncProcessingAsync(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess, TimeSpan? timeDelay)
        {
            string[] queues = eventsToProcess.Select(x => x.QueueName).Distinct().ToArray();
            var jobs = queues.Select(x => new ProcessAsyncEventsJob(x, AsyncEventPipelineConfiguration.Current.AsyncProcessAttemptCount, AsyncEventPipelineConfiguration.Current.AsyncProcessRetryTimeout));

            foreach (ProcessAsyncEventsJob job in jobs)
            {
                await jobScheduler.EnqeueJobAsync(job, timeDelay);
            }
        }

        private async Task<string[]> TryRunQueues(string[] queues)
        {
            string[] finishedQueues = await Task.WhenAll(queues.Select(x =>
                Task.Factory.StartNewWithContext(async () =>
                {
                    try
                    {
                        var asyncEventQueueBacklogWorker = asyncEventQueueBacklogWorkerFunc();
                        await asyncEventQueueBacklogWorker.RunQueueBacklogAsync(x);
                        return x;
                    }
                    catch (AsyncEventProcessingSequenceException e)
                    {
                        Logger.Debug(e,
                            $"AsyncEventProcessingSequenceException occurred during synchronous queue processing");
                        return null; //can retry
                    }
                    catch (OptimisticConcurrencyException e)
                    {
                        Logger.Debug(e, $"OptimisticConcurrencyException occurred during synchronous queue processing");
                        return null; //can retry
                    }
                })
            ));

            return queues.Except(finishedQueues).ToArray();
        }
    }
}
