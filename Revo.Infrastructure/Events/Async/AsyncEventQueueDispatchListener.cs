using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventQueueDispatchListener : IEventListener<IEvent>, IUnitOfWorkListener
    {
        private readonly IAsyncEventQueueDispatcher asyncEventQueueDispatcher;
        private readonly IAsyncEventProcessor asyncEventProcessor;
        private readonly List<IEventMessage<IEvent>> eventMessages = new List<IEventMessage<IEvent>>();

        public AsyncEventQueueDispatchListener(IAsyncEventQueueDispatcher asyncEventQueueDispatcher,
            IAsyncEventProcessor asyncEventProcessor)
        {
            this.asyncEventQueueDispatcher = asyncEventQueueDispatcher;
            this.asyncEventProcessor = asyncEventProcessor;
        }

        public Task HandleAsync(IEventMessage<IEvent> message, CancellationToken cancellationToken)
        {
            eventMessages.Add(message);
            return Task.FromResult(0);
        }

        public Task OnBeforeWorkCommitAsync(IUnitOfWork unitOfWork)
        {
            return Task.CompletedTask;
        }

        public void OnWorkBegin(IUnitOfWork unitOfWork)
        {
        }

        public async Task OnWorkSucceededAsync(IUnitOfWork unitOfWork)
        {
            var dispatchResult = await asyncEventQueueDispatcher.DispatchToQueuesAsync(eventMessages, null, null);
            eventMessages.Clear();

            if (dispatchResult.EnqueuedEventsAsyncProcessed.Count > 0)
            {
                await asyncEventProcessor.EnqueueForAsyncProcessingAsync(dispatchResult.EnqueuedEventsAsyncProcessed, null);
            }

            if (dispatchResult.EnqueuedEventsSyncProcessed.Count > 0)
            {
                await asyncEventProcessor.ProcessSynchronously(dispatchResult.EnqueuedEventsSyncProcessed);
            }
        }
    }
}
