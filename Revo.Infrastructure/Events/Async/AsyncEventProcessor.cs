using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Jobs.InMemory;

namespace Revo.Infrastructure.Events.Async;

public class AsyncEventProcessor(
    IServiceScopeFactory serviceScopeFactory,
    IAsyncEventQueueManager asyncEventQueueManager,
    IInMemoryJobScheduler jobScheduler,
    IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration,
    ILogger<AsyncEventProcessor> logger)
    : IAsyncEventProcessor
{
    public async Task ProcessSynchronously(IReadOnlyCollection<IAsyncEventQueueRecord> eventsToProcess)
    {
        var queues = eventsToProcess.Select(x => x.QueueName).Distinct().ToArray();
        var remainingEvents = eventsToProcess.ToList();

        var triesLeft = asyncEventPipelineConfiguration.SyncProcessAttemptCount;
        var sleepTime = asyncEventPipelineConfiguration.SyncProcessRetryTimeout;
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
        var queues = eventsToProcess.Select(x => x.QueueName).Distinct().ToArray();
        var jobs = queues.Select(x => new ProcessAsyncEventsJob(x, asyncEventPipelineConfiguration.AsyncProcessAttemptCount, asyncEventPipelineConfiguration.AsyncProcessRetryTimeout));

        foreach (var job in jobs)
        {
            await jobScheduler.EnqeueJobAsync(job, timeDelay);
        }
    }

    private async Task<string[]> TryRunQueues(string[] queues)
    {
        var throttledTasks = new List<Task<string>>();

        using (var throttle = new SemaphoreSlim(asyncEventPipelineConfiguration.SyncQueueProcessingParallelism))
        {
            foreach (var queueName in queues)
            {
                await throttle.WaitAsync();

                throttledTasks.Add(Task.Run(async () =>
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    
                    try
                    {
                        var asyncEventQueueBacklogWorker = 
                            scope.ServiceProvider.GetRequiredService<IAsyncEventWorker>();
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

            var finishedQueues = await Task.WhenAll(throttledTasks);
            return queues.Except(finishedQueues).ToArray();
        }
    }
}