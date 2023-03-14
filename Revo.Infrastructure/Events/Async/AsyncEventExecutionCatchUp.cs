using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventExecutionCatchUp : IApplicationStartedListener
    {
        private readonly IEventSourceCatchUp[] eventSourceCatchUps;
        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration;
        private readonly Func<IAsyncEventWorker> asyncEventWorkerFunc;
        private readonly ILogger logger;

        public AsyncEventExecutionCatchUp(IEventSourceCatchUp[] eventSourceCatchUps,
            IAsyncEventQueueManager asyncEventQueueManager,
            IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration,
            Func<IAsyncEventWorker> asyncEventWorkerFunc,
            ILogger logger)
        {
            this.eventSourceCatchUps = eventSourceCatchUps;
            this.asyncEventQueueManager = asyncEventQueueManager;
            this.asyncEventPipelineConfiguration = asyncEventPipelineConfiguration;
            this.asyncEventWorkerFunc = asyncEventWorkerFunc;
            this.logger = logger;
        }

        public void OnApplicationStarted()
        {
            var task = Task.Run(InitializeAsync);

            if (asyncEventPipelineConfiguration.WaitForEventCatchUpsUponStartup)
            {
                task.GetAwaiter().GetResult();
            }
        }

        private async Task InitializeAsync()
        {
            await CatchUpEventSourcesAsync();
            await RunBackloggedQueuesAsync();
        }

        private async Task CatchUpEventSourcesAsync()
        {
            foreach (var eventSourceCatchUp in eventSourceCatchUps)
            {
                try
                {
                    await eventSourceCatchUp.CatchUpAsync();
                }
                catch (Exception e)
                {
                    // TODO reschedule?
                    logger.LogError(e, $"Failed to catch-up an event source {eventSourceCatchUp} during startup");
                }
            }
        }
        
        private async Task RunBackloggedQueuesAsync()
        {
            var backloggedQueueNames = await asyncEventQueueManager.GetNonemptyQueueNamesAsync();
            var throttledTasks = new List<Task>();

            using (var throttle = new SemaphoreSlim(asyncEventPipelineConfiguration.CatchUpProcessingParallelism))
            {
                foreach (string queueName in backloggedQueueNames)
                {
                    await throttle.WaitAsync();

                    throttledTasks.Add(Task.Factory.StartNewWithContext(async () =>
                    {
                        try
                        {
                            var asyncEventQueueBacklogWorker = asyncEventWorkerFunc();
                            await asyncEventQueueBacklogWorker.RunQueueBacklogAsync(queueName);
                        }
                        catch (AsyncEventProcessingException e)
                        {
                            // TODO reschedule?
                            logger.LogError(e,
                                $"AsyncEventProcessingException occurred while processing async event queue {queueName} during startup");
                        }
                        finally
                        {
                            throttle.Release();
                        }
                    }));
                }

                await Task.WhenAll(throttledTasks);
            }
        }
    }
}
