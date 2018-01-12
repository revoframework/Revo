using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core.Lifecycle;

namespace GTRevo.Infrastructure.Events.Async
{
    public class AsyncEventExecutionCatchUp : IApplicationStartListener
    {
        private readonly IEventSourceCatchUp[] eventSourceCatchUps;
        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly IAsyncEventQueueBacklogWorker asyncEventQueueBacklogWorker;

        public AsyncEventExecutionCatchUp(IEventSourceCatchUp[] eventSourceCatchUps,
            IAsyncEventQueueManager asyncEventQueueManager,
            IAsyncEventQueueBacklogWorker asyncEventQueueBacklogWorker)
        {
            this.eventSourceCatchUps = eventSourceCatchUps;
            this.asyncEventQueueManager = asyncEventQueueManager;
            this.asyncEventQueueBacklogWorker = asyncEventQueueBacklogWorker;
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
                await eventSourceCatchUp.CatchUpAsync();
            }
        }
        
        private async Task RunBackloggedQueuesAsync()
        {
            var backloggedQueueNames = await asyncEventQueueManager.GetNonemptyQueueNamesAsync();
            foreach (string queueName in backloggedQueueNames)
            {
                await asyncEventQueueBacklogWorker.RunQueueBacklogAsync(queueName);
            }
        }
    }
}
