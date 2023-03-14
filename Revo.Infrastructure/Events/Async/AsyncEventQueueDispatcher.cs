using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Revo.Core.Core;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventQueueDispatcher : IAsyncEventQueueDispatcher
    {
        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly IServiceLocator serviceLocator;

        public AsyncEventQueueDispatcher(IAsyncEventQueueManager asyncEventQueueManager, IServiceLocator serviceLocator)
        {
            this.asyncEventQueueManager = asyncEventQueueManager;
            this.serviceLocator = serviceLocator;
        }

        public Task<string> GetLastEventSourceDispatchCheckpointAsync(string eventSourceName)
        {
            return asyncEventQueueManager.GetEventSourceCheckpointAsync(eventSourceName);
        }

        public async Task<QueueDispatchResult> DispatchToQueuesAsync(IEnumerable<IEventMessage> eventMessages,
            string eventSourceName, string eventSourceCheckpoint)
        {
            var queuesForAsyncDispatch = new HashSet<string>();
            var queuesForSyncDispatch = new HashSet<string>();

            foreach (var message in eventMessages)
            {
                var queues = GetEventQueues(message);

                // we need to call EnqueueEventAsync even when queues are empty so that the event record gets marked as dispatched
                await asyncEventQueueManager.EnqueueEventAsync(message, queues.Select(x => x.Sequencing).ToArray());
                queues.Where(x => !x.SynchronousDispatch).ForEach(x => queuesForAsyncDispatch.Add(x.Sequencing.SequenceName));
                queues.Where(x => x.SynchronousDispatch).ForEach(x => queuesForSyncDispatch.Add(x.Sequencing.SequenceName));
            }

            if (eventSourceName != null)
            {
                await asyncEventQueueManager.SetEventSourceCheckpointAsync(eventSourceName, eventSourceCheckpoint);
            }

            var enqueuedEvents = await asyncEventQueueManager.CommitAsync();

            return new QueueDispatchResult()
            {
                EnqueuedEventsAsyncProcessed = enqueuedEvents.Where(x => queuesForAsyncDispatch.Contains(x.QueueName)).ToList(),
                EnqueuedEventsSyncProcessed = enqueuedEvents.Where(x => queuesForSyncDispatch.Contains(x.QueueName)).ToList()
            };
        }

        private IReadOnlyCollection<(EventSequencing Sequencing, bool SynchronousDispatch)> GetEventQueues(IEventMessage message)
        {
            var eventSequencerType = typeof(IAsyncEventSequencer<>).MakeGenericType(message.Event.GetType());
            var eventSequencers = serviceLocator.GetAll(eventSequencerType).Cast<IAsyncEventSequencer>();
            
            var allQueues = new Dictionary<string, (EventSequencing sequencing, bool synchronousDispatch)>();
            foreach (IAsyncEventSequencer eventSequencer in eventSequencers)
            {
                var sequences = eventSequencer.GetEventSequencing(message);
                bool synchronousDispatch = eventSequencer.ShouldAttemptSynchronousDispatch(message);
                foreach (var sequence in sequences)
                {
                    string sequenceName = sequence.SequenceName ?? "null";
                    allQueues[sequenceName] = (sequence, synchronousDispatch);
                }
            }

            return allQueues.Values;
        }
    }
}
