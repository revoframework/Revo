using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.EF6.EventStore.Model;
using GTRevo.Infrastructure.Events.Async;
using GTRevo.Infrastructure.EventStore;
using EntityState = GTRevo.DataAccess.Entities.EntityState;

namespace GTRevo.Infrastructure.EF6.Events.Async
{
    public class AsyncEventQueueManager : IAsyncEventQueueManager
    {
        private readonly IEF6CrudRepository ef6CrudRepository;
        private readonly IDomainEventTypeCache domainEventTypeCache;

        private readonly List<(ExternalEventRecord externalEventRecord, List<QueuedAsyncEvent> queuedEvents)>
            externalEvents = new List<(ExternalEventRecord externalEventRecord, List<QueuedAsyncEvent> queuedEvents)>();

        public AsyncEventQueueManager(IEF6CrudRepository ef6CrudRepository, IDomainEventTypeCache domainEventTypeCache)
        {
            this.ef6CrudRepository = ef6CrudRepository;
            this.domainEventTypeCache = domainEventTypeCache;
        }

        public async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> FindQueuedEventsAsync(Guid[] asyncEventQueueRecordIds)
        {
            return await ef6CrudRepository.FindAll<QueuedAsyncEvent>()
                .Where(x => asyncEventQueueRecordIds.Contains(x.Id))
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<string>> GetNonemptyQueueNamesAsync()
        {
            return await ef6CrudRepository.FindAll<QueuedAsyncEvent>()
                .Select(x => x.QueueId)
                .Distinct()
                .ToListAsync();
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

            var events = await ef6CrudRepository
                .Where<QueuedAsyncEvent>(x => x.QueueId == queue.Id)
                .OrderBy(x => x.SequenceNumber)
                .Include(x => x.EventStreamRow)
                .Include(x => x.ExternalEventRecord)
                .ToListAsync();

            return events;
        }

        public async Task DequeueEventAsync(Guid asyncEventQueueRecordId)
        {
            QueuedAsyncEvent queuedEvent = await ef6CrudRepository.FindAsync<QueuedAsyncEvent>(asyncEventQueueRecordId);
            if (queuedEvent != null)
            {
                ef6CrudRepository.Remove(queuedEvent);

                AsyncEventQueue queue = await GetOrCreateQueueAsync(queuedEvent.QueueName, null);
                if (queuedEvent.SequenceNumber != null)
                {
                    queue.LastSequenceNumberProcessed = queuedEvent.SequenceNumber;
                }
            }
        }

        Task<IReadOnlyCollection<IAsyncEventQueueRecord>> IAsyncEventQueueManager.EnqueueEventAsync(IEventMessage eventMessage, IEnumerable<EventSequencing> queues)
        {
            throw new NotImplementedException();
        }

        public async Task EnqueueEventAsync(IEventMessage eventMessage, IEnumerable<EventSequencing> queues)
        {
            EventStreamRow eventStreamRow = null;
            ExternalEventRecord externalEventRecord = null;
            if (eventMessage is IEventStoreEventMessage eventStoreEventMessage)
            {
                eventStreamRow = eventStoreEventMessage.Record as EventStreamRow;
                if (eventStreamRow.IsDispatchedToAsyncQueues)
                {
                    return; // TODO might want to return existing queue records instead?
                }

                eventStreamRow.MarkDispatchedToAsyncQueues();

                if (!ef6CrudRepository.IsAttached(eventStreamRow))
                {
                    ef6CrudRepository.Attach(eventStreamRow);
                }
            }
            else
            {
                externalEventRecord = new ExternalEventRecord(domainEventTypeCache,
                    eventMessage.Event, eventMessage.Metadata);
            }

            List<QueuedAsyncEvent> externalQueuedEvents = new List<QueuedAsyncEvent>();
            foreach (var sequence in queues)
            {
                AsyncEventQueue queue = await GetOrCreateQueueAsync(sequence.SequenceName, null);
                if (eventStreamRow != null)
                {
                    QueuedAsyncEvent queuedEvent = new QueuedAsyncEvent(queue.Id, eventStreamRow, sequence.EventSequenceNumber);
                    ef6CrudRepository.Add(queuedEvent);
                }
                else
                {
                    externalQueuedEvents.Add(new QueuedAsyncEvent(queue.Id, externalEventRecord, sequence.EventSequenceNumber));
                }
            }

            if (externalQueuedEvents.Count > 0)
            {
                if (externalEvents.Any(x => x.externalEventRecord.Id == externalEventRecord.Id))
                {
                    throw new InvalidOperationException($"Tried to enqueue external event to async queue twice, event ID: {externalEventRecord.Id}");
                }

                externalEvents.Add((externalEventRecord, externalQueuedEvents));
            }
        }

        public async Task<string> GetEventSourceCheckpointAsync(string eventSourceName)
        {
            EventSourceState eventSourceState = await ef6CrudRepository.FindAsync<EventSourceState>(eventSourceName);
            return eventSourceState?.EventEnqueueCheckpoint;
        }

        public async Task SetEventSourceCheckpointAsync(string eventSourceName, string opaqueCheckpoint)
        {
            EventSourceState eventSourceState = await ef6CrudRepository.FindAsync<EventSourceState>(eventSourceName);
            if (eventSourceState == null)
            {
                eventSourceState = new EventSourceState(eventSourceName);
                ef6CrudRepository.Add(eventSourceState);
            }

            eventSourceState.EventEnqueueCheckpoint = opaqueCheckpoint;
        }

        public async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> CommitAsync()
        {
            await ResolveExternalEvents();
            List<QueuedAsyncEvent> queueRecords = ef6CrudRepository.GetEntities<QueuedAsyncEvent>(EntityState.Added).ToList();
            await ef6CrudRepository.SaveChangesAsync();
            return queueRecords;
        }

        private async Task ResolveExternalEvents()
        {
            var externalEventIds = externalEvents.Select(x => x.externalEventRecord.Id).ToArray();
            var existingRecordIds = await ef6CrudRepository
                .Where<ExternalEventRecord>(x => externalEventIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var externalEventPair in externalEvents)
            {
                if (existingRecordIds.Contains(externalEventPair.externalEventRecord.Id))
                {
                    continue;
                }

                ef6CrudRepository.AddRange(externalEventPair.queuedEvents);
            }

            externalEvents.Clear();
        }

        private Task<AsyncEventQueue> GetQueueAsync(string queueName)
        {
            return ef6CrudRepository.FindAsync<AsyncEventQueue>(queueName);
        }

        private async Task<AsyncEventQueue> GetOrCreateQueueAsync(string queueName, long? lastSequenceNumberProcessed)
        {
            AsyncEventQueue queue = await GetQueueAsync(queueName);
            if (queue == null)
            {
                queue = new AsyncEventQueue(queueName, lastSequenceNumberProcessed);
                ef6CrudRepository.Add(queue);
            }

            return queue;
        }
    }
}
