using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventExecutionCatchUp : IApplicationStartListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEventSourceCatchUp[] eventSourceCatchUps;
        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly Func<IAsyncEventWorker> asyncEventQueueBacklogWorkerFunc;

        public AsyncEventExecutionCatchUp(IEventSourceCatchUp[] eventSourceCatchUps,
            IAsyncEventQueueManager asyncEventQueueManager,
            Func<IAsyncEventWorker> asyncEventQueueBacklogWorkerFunc)
        {
            this.eventSourceCatchUps = eventSourceCatchUps;
            this.asyncEventQueueManager = asyncEventQueueManager;
            this.asyncEventQueueBacklogWorkerFunc = asyncEventQueueBacklogWorkerFunc;
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

            await Task.WhenAll(backloggedQueueNames.Select(queueName =>
                Task.Factory.StartNewWithContext(async () =>
                {
                    var asyncEventQueueBacklogWorker = asyncEventQueueBacklogWorkerFunc();
                    try
                    {
                        await asyncEventQueueBacklogWorker.RunQueueBacklogAsync(queueName);
                    }
                    catch (AsyncEventProcessingException e)
                    {
                        // TODO reschedule?
                        Logger.Error(e, $"AsyncEventProcessingException occurred while processing async event queue {queueName} during startup");
                    }
                }))); //using new Task because we want a new context (parallelization on ASP.NET 4 + fresh DI lifetime scope)
        }
    }
}
