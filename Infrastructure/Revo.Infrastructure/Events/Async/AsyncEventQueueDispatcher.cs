using System;
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
            HashSet<string> queuesForAsyncDispatch = new HashSet<string>();
            HashSet<string> queuesForSyncDispatch = new HashSet<string>();

            foreach (var message in eventMessages)
            {
                var queues = GetEventQueues(message);
                await asyncEventQueueManager.EnqueueEventAsync(message, queues.Select(x => x.sequencing));
                queues.Where(x => !x.synchronousDispatch).ForEach(x => queuesForAsyncDispatch.Add(x.sequencing.SequenceName));
                queues.Where(x => x.synchronousDispatch).ForEach(x => queuesForSyncDispatch.Add(x.sequencing.SequenceName));
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

        private IEnumerable<(EventSequencing sequencing, bool synchronousDispatch)> GetEventQueues(IEventMessage message)
        {
            Type eventSequencerType = typeof(IAsyncEventSequencer<>).MakeGenericType(message.Event.GetType());
            IEnumerable<IAsyncEventSequencer> eventSequencers = serviceLocator.GetAll(eventSequencerType).Cast<IAsyncEventSequencer>();
            
            Dictionary<string, (EventSequencing sequencing, bool synchronousDispatch)> allQueues = new Dictionary<string, (EventSequencing sequencing, bool synchronousDispatch)>();
            foreach (IAsyncEventSequencer eventSequencer in eventSequencers)
            {
                IEnumerable<EventSequencing> sequences = eventSequencer.GetEventSequencing(message);
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
