using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using MoreLinq;

namespace GTRevo.Infrastructure.Events.Async
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
            return Task.FromResult(0);
        }

        public void OnWorkBegin(IUnitOfWork unitOfWork)
        {
        }

        public async Task OnWorkSucceededAsync(IUnitOfWork unitOfWork)
        {
            var dispatchResult = await asyncEventQueueDispatcher.DispatchToQueuesAsync(eventMessages, null, null);
            eventMessages.Clear();

            if (dispatchResult.EnqueuedEventsAsyncProcessed.Any())
            {
                await asyncEventProcessor.EnqueueForAsyncProcessingAsync(dispatchResult.EnqueuedEventsAsyncProcessed, null);
            }

            if (dispatchResult.EnqueuedEventsSyncProcessed.Any())
            {
                await asyncEventProcessor.ProcessSynchronously(dispatchResult.EnqueuedEventsSyncProcessed);
            }
        }
    }
}
