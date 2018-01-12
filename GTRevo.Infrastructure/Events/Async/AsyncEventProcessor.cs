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

        private readonly IAsyncEventQueueBacklogWorker asyncEventQueueBacklogWorker;
        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly IJobScheduler jobScheduler;

        public AsyncEventProcessor(IAsyncEventQueueBacklogWorker asyncEventQueueBacklogWorker,
            IAsyncEventQueueManager asyncEventQueueManager,
            IJobScheduler jobScheduler)
        {
            this.asyncEventQueueBacklogWorker = asyncEventQueueBacklogWorker;
            this.asyncEventQueueManager = asyncEventQueueManager;
            this.jobScheduler = jobScheduler;
        }

        public async Task ProcessSynchronously(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess)
        {
            string[] queues = eventsToProcess.Select(x => x.QueueName).Distinct().ToArray();
            List<IAsyncEventQueueRecord> remainingEvents = eventsToProcess.ToList();

            int triesLeft = 3;
            TimeSpan sleepTime = TimeSpan.FromMilliseconds(500);
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
                    sleepTime = TimeSpan.FromTicks(sleepTime.Ticks * 4);
                }
            }

            if (queues.Length > 0 && remainingEvents.Count > 0)
            {
                Logger.Error(
                    $"Not able to synchronously process all event queues, about to reschedule {remainingEvents.Count} events for later async processing in 1 minute");
                await EnqueueForAsyncProcessing(remainingEvents, TimeSpan.FromMinutes(1), TimeSpan.FromMilliseconds(500));
            }
        }

        public async Task EnqueueForAsyncProcessing(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess, TimeSpan? timeDelay,
            TimeSpan retryTimeout)
        {
            string[] queues = eventsToProcess.Select(x => x.QueueName).Distinct().ToArray();
            var jobs = queues.Select(x => new ProcessAsyncEventsJob(x, 3, retryTimeout));

            foreach (ProcessAsyncEventsJob job in jobs)
            {
                await jobScheduler.EnqeueJobAsync(job, timeDelay);
            }
        }

        private async Task<string[]> TryRunQueues(string[] queues)
        {
            string[] finishedQueues = await Task.WhenAll(queues.Select(async x =>
            {
                try
                {
                    await asyncEventQueueBacklogWorker.RunQueueBacklogAsync(x);
                    return x;
                }
                catch (AsyncEventProcessingSequenceException e)
                {
                    Logger.Debug(e, $"AsyncEventProcessingSequenceException occurred during synchronous queue processing");
                    return null; //can retry
                }
                catch (OptimisticConcurrencyException e)
                {
                    Logger.Debug(e, $"OptimisticConcurrencyException occurred during synchronous queue processing");
                    return null; //can retry
                }
            }));

            return queues.Except(finishedQueues).ToArray();
        }
    }
}
