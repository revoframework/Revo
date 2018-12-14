using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Revo.Core.Events;
using Revo.Core.Types;
using Revo.DataAccess.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.EventStores.Generic;
using Revo.Infrastructure.EventStores.Generic.Model;
using EntityState = Revo.DataAccess.Entities.EntityState;

namespace Revo.Infrastructure.Events.Async.Generic
{
    public class AsyncEventQueueManager : IAsyncEventQueueManager
    {
        private readonly ICrudRepository crudRepository;
        private readonly IEventSerializer eventSerializer;
        private readonly IExternalEventStore externalEventStore;

        private readonly Dictionary<IEventMessage, List<EventSequencing>> eventsToQueues = new Dictionary<IEventMessage, List<EventSequencing>>();

        public AsyncEventQueueManager(ICrudRepository crudRepository, IEventSerializer eventSerializer,
            IExternalEventStore externalEventStore)
        {
            this.crudRepository = crudRepository;
            this.eventSerializer = eventSerializer;
            this.externalEventStore = externalEventStore;
        }

        public async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> FindQueuedEventsAsync(Guid[] asyncEventQueueRecordIds)
        {
            return (await QueryQueuedEvents()
                .Where(x => asyncEventQueueRecordIds.Contains(x.Id))
                .ToListAsync(crudRepository))
                .Select(SelectRecordFromQueuedEvent)
                .ToList();
        }

        public async Task<IReadOnlyCollection<string>> GetNonemptyQueueNamesAsync()
        {
            return await crudRepository.FindAll<QueuedAsyncEvent>()
                .Select(x => x.QueueId)
                .Distinct()
                .ToListAsync(crudRepository);
        }

        public async Task<IAsyncEventQueueState> GetQueueStateAsync(string queueName)
        {
            AsyncEventQueue queue = await GetQueueAsync(queueName);
            return queue; //can be null
        }

        public async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> GetQueueEventsAsync(string queueName)
        {
            AsyncEventQueue queue = await GetQueueAsync(queueName);
            if (queue == null)
            {
                return new List<IAsyncEventQueueRecord>();
            }

            var events = (await QueryQueuedEvents()
                .Where(x => x.QueueId == queue.Id)
                .ToListAsync(crudRepository))
                .Select(SelectRecordFromQueuedEvent)
                .ToList();
            
            return events;
        }

        public async Task DequeueEventAsync(Guid asyncEventQueueRecordId)
        {
            QueuedAsyncEvent queuedEvent = await crudRepository.FindAsync<QueuedAsyncEvent>(asyncEventQueueRecordId);
            if (queuedEvent != null)
            {
                AsyncEventQueue queue = await GetOrCreateQueueAsync(queuedEvent.QueueId, null);
                if (queuedEvent.SequenceNumber != null)
                {
                    queue.LastSequenceNumberProcessed = queuedEvent.SequenceNumber;
                }

                crudRepository.Remove(queuedEvent);
            }
        }
        
        public async Task EnqueueEventAsync(IEventMessage eventMessage, IEnumerable<EventSequencing> queues)
        {
            if (!eventsToQueues.TryGetValue(eventMessage, out var queueList))
            {
                queueList = eventsToQueues[eventMessage] = new List<EventSequencing>();
            }

            queueList.AddRange(queues);
        }
        
        public async Task<string> GetEventSourceCheckpointAsync(string eventSourceName)
        {
            EventSourceState eventSourceState = await crudRepository.FindAsync<EventSourceState>(eventSourceName);
            return eventSourceState?.EventEnqueueCheckpoint;
        }

        public async Task SetEventSourceCheckpointAsync(string eventSourceName, string opaqueCheckpoint)
        {
            EventSourceState eventSourceState = await crudRepository.FindAsync<EventSourceState>(eventSourceName);
            if (eventSourceState == null)
            {
                eventSourceState = new EventSourceState(eventSourceName);
                crudRepository.Add(eventSourceState);
            }

            eventSourceState.EventEnqueueCheckpoint = opaqueCheckpoint;
        }

        public virtual async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> CommitAsync()
        {
            List<QueuedAsyncEvent> queuedEvents = await EnqueueEventsToRepositoryAsync();
            await crudRepository.SaveChangesAsync();
            return queuedEvents.Select(SelectRecordFromQueuedEvent).ToList();
        }

        protected async Task<List<QueuedAsyncEvent>> EnqueueEventsToRepositoryAsync()
        {
            var eventStoreEvents = eventsToQueues.Where(x =>
                (x.Key as IEventStoreEventMessage)?.Record is EventStoreRecordAdapter).ToArray();
            var externalEvents = eventsToQueues.Except(eventStoreEvents).ToArray();

            var queuedEvents = new List<QueuedAsyncEvent>();

            //external
            if (externalEvents.Length > 0)
            {
                foreach (var externalEvent in externalEvents)
                {
                    externalEventStore.PushEvent(externalEvent.Key);
                }

                var externalEventRecords = await externalEventStore.CommitAsync();
                foreach (var externalEventRecord in externalEventRecords)
                {
                    if (externalEventRecord.IsDispatchedToAsyncQueues)
                    {
                        continue;
                    }

                    externalEventRecord.MarkDispatchedToAsyncQueues();
                    var sequences = externalEvents.First(x => x.Key.Metadata.GetEventId() == externalEventRecord.Id).Value;

                    foreach (var sequence in sequences)
                    {
                        AsyncEventQueue queue = await GetOrCreateQueueAsync(sequence.SequenceName, null);
                        QueuedAsyncEvent queuedEvent = new QueuedAsyncEvent(queue.Id, externalEventRecord, sequence.EventSequenceNumber);
                        crudRepository.Add(queuedEvent);
                        queuedEvents.Add(queuedEvent);
                    }
                }
            }

            //event store
            foreach (var eventToQueues in eventStoreEvents)
            {
                var storeMessage = (IEventStoreEventMessage) eventToQueues.Key;
                var eventStreamRow = ((EventStoreRecordAdapter)storeMessage.Record).EventStreamRow;
                if (eventStreamRow.IsDispatchedToAsyncQueues)
                {
                    continue;
                }

                eventStreamRow.MarkDispatchedToAsyncQueues();

                if (!crudRepository.IsAttached(eventStreamRow))
                {
                    crudRepository.Attach(eventStreamRow);
                    crudRepository.SetEntityState(eventStreamRow, EntityState.Modified);
                }

                foreach (var sequence in eventToQueues.Value)
                {
                    AsyncEventQueue queue = await GetOrCreateQueueAsync(sequence.SequenceName, null);
                    QueuedAsyncEvent queuedEvent = new QueuedAsyncEvent(queue.Id, eventStreamRow, sequence.EventSequenceNumber);
                    crudRepository.Add(queuedEvent);
                    queuedEvents.Add(queuedEvent);
                }
            }

            eventsToQueues.Clear();
            return queuedEvents;
        }

        private Task<AsyncEventQueue> GetQueueAsync(string queueName)
        {
            return crudRepository.FindAsync<AsyncEventQueue>(queueName);
        }

        private async Task<AsyncEventQueue> GetOrCreateQueueAsync(string queueName, long? lastSequenceNumberProcessed)
        {
            AsyncEventQueue queue = await GetQueueAsync(queueName);
            if (queue == null)
            {
                queue = new AsyncEventQueue(queueName, lastSequenceNumberProcessed);
                crudRepository.Add(queue);
            }

            return queue;
        }
        
        private IQueryable<QueuedAsyncEvent> QueryQueuedEvents()
        {
            return crudRepository
                .FindAll<QueuedAsyncEvent>()
                .OrderBy(x => x.SequenceNumber)
                .Include(crudRepository, x => x.EventStreamRow)
                .Include(crudRepository, x => x.ExternalEventRecord);
        }

        protected AsyncEventQueueRecordAdapter SelectRecordFromQueuedEvent(QueuedAsyncEvent queuedEvent)
        {
            return new AsyncEventQueueRecordAdapter(queuedEvent, eventSerializer);
        }
    }
}

