using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventExecutionCatchUp : IApplicationStartListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEventSourceCatchUp[] eventSourceCatchUps;
        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration;
        private readonly Func<IAsyncEventWorker> asyncEventWorkerFunc;

        public AsyncEventExecutionCatchUp(IEventSourceCatchUp[] eventSourceCatchUps,
            IAsyncEventQueueManager asyncEventQueueManager,
            IAsyncEventPipelineConfiguration asyncEventPipelineConfiguration,
            Func<IAsyncEventWorker> asyncEventWorkerFunc)
        {
            this.eventSourceCatchUps = eventSourceCatchUps;
            this.asyncEventQueueManager = asyncEventQueueManager;
            this.asyncEventPipelineConfiguration = asyncEventPipelineConfiguration;
            this.asyncEventWorkerFunc = asyncEventWorkerFunc;
        }

        public void OnApplicationStarted()
        {
            Task.Run(InitializeAsync).GetAwaiter().GetResult();
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
                    Logger.Error(e, $"Failed to catch-up an event source {eventSourceCatchUp} during startup");
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
                            Logger.Error(e,
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
