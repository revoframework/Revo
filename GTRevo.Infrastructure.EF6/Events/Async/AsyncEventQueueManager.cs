using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.EF6.EventStore.Model;
using GTRevo.Infrastructure.Events.Async;
using GTRevo.Infrastructure.EventStore;
using EntityState = GTRevo.DataAccess.Entities.EntityState;

namespace GTRevo.Infrastructure.EF6.Events.Async
{
    public class AsyncEventQueueManager : IAsyncEventQueueManager
    {
        private readonly ICrudRepository crudRepository;
        private readonly IDomainEventTypeCache domainEventTypeCache;

        private readonly List<(ExternalEventRecord externalEventRecord, List<QueuedAsyncEvent> queuedEvents)>
            externalEvents = new List<(ExternalEventRecord externalEventRecord, List<QueuedAsyncEvent> queuedEvents)>();

        public AsyncEventQueueManager(ICrudRepository crudRepository, IDomainEventTypeCache domainEventTypeCache)
        {
            this.crudRepository = crudRepository;
            this.domainEventTypeCache = domainEventTypeCache;
        }

        public async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> FindQueuedEventsAsync(Guid[] asyncEventQueueRecordIds)
        {
            return await QueryQueuedEvents()
                .Where(x => asyncEventQueueRecordIds.Contains(x.Id))
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<string>> GetNonemptyQueueNamesAsync()
        {
            return await crudRepository.FindAll<QueuedAsyncEvent>()
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

            var events = await QueryQueuedEvents()
                .Where(x => x.QueueId == queue.Id)
                .ToListAsync();

            foreach (var @event in events)
            {
                if (@event.EventStreamRow != null && @event.EventStreamRow.DomainEventTypeCache == null)
                {
                    @event.EventStreamRow.DomainEventTypeCache = domainEventTypeCache;
                }
                else if (@event.ExternalEventRecord != null && @event.ExternalEventRecord.DomainEventTypeCache == null)
                {
                    @event.ExternalEventRecord.DomainEventTypeCache = domainEventTypeCache;
                }
            }

            return events;
        }

        public async Task DequeueEventAsync(Guid asyncEventQueueRecordId)
        {
            QueuedAsyncEvent queuedEvent = await crudRepository.FindAsync<QueuedAsyncEvent>(asyncEventQueueRecordId);
            if (queuedEvent != null)
            {
                AsyncEventQueue queue = await GetOrCreateQueueAsync(queuedEvent.QueueName, null);
                if (queuedEvent.SequenceNumber != null)
                {
                    queue.LastSequenceNumberProcessed = queuedEvent.SequenceNumber;
                }

                crudRepository.Remove(queuedEvent);
            }
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

                if (!crudRepository.IsAttached(eventStreamRow))
                {
                    crudRepository.Attach(eventStreamRow);
                    crudRepository.SetEntityState(eventStreamRow, EntityState.Modified);
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
                    crudRepository.Add(queuedEvent);
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

        public async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> CommitAsync()
        {
            await ResolveExternalEvents();
            List<QueuedAsyncEvent> queueRecords = crudRepository.GetEntities<QueuedAsyncEvent>(EntityState.Added).ToList();
            await crudRepository.SaveChangesAsync();
            return queueRecords;
        }

        private async Task ResolveExternalEvents()
        {
            var externalEventIds = externalEvents.Select(x => x.externalEventRecord.Id).ToArray();
            var existingRecordIds = await crudRepository
                .Where<ExternalEventRecord>(x => externalEventIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var externalEventPair in externalEvents)
            {
                if (existingRecordIds.Contains(externalEventPair.externalEventRecord.Id))
                {
                    continue;
                }

                crudRepository.AddRange(externalEventPair.queuedEvents);
            }

            externalEvents.Clear();
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
                .Include(x => x.EventStreamRow)
                .Include(x => x.ExternalEventRecord);
        }
    }
}
