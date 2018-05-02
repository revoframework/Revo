using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Types;
using Revo.DataAccess.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.EF6.EventStore;
using Revo.Infrastructure.EF6.EventStore.Model;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventStore;
using EntityState = Revo.DataAccess.Entities.EntityState;

namespace Revo.Infrastructure.EF6.Events.Async
{
    public class AsyncEventQueueManager : IAsyncEventQueueManager
    {
        private readonly ICrudRepository crudRepository;
        private readonly IEventSerializer eventSerializer;

        private readonly List<(ExternalEventRecord externalEventRecord, List<QueuedAsyncEvent> queuedEvents)>
            externalEvents = new List<(ExternalEventRecord externalEventRecord, List<QueuedAsyncEvent> queuedEvents)>();

        public AsyncEventQueueManager(ICrudRepository crudRepository, IEventSerializer eventSerializer)
        {
            this.crudRepository = crudRepository;
            this.eventSerializer = eventSerializer;
        }

        public async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> FindQueuedEventsAsync(Guid[] asyncEventQueueRecordIds)
        {
            return (await QueryQueuedEvents()
                .Where(x => asyncEventQueueRecordIds.Contains(x.Id))
                .ToListAsync())
                .Select(SelectRecordFromQueuedEvent)
                .ToList();
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

            var events = (await QueryQueuedEvents()
                .Where(x => x.QueueId == queue.Id)
                .ToListAsync())
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
            EventStreamRow eventStreamRow = null;
            ExternalEventRecord externalEventRecord = null;
            if (eventMessage is IEventStoreEventMessage eventStoreEventMessage)
            {
                eventStreamRow = ((EventStoreRecordAdapter)eventStoreEventMessage.Record).EventStreamRow;
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
                externalEventRecord = CreateExternalEventRecord(eventMessage);
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
            return queueRecords.Select(SelectRecordFromQueuedEvent).ToList();
        }

        private ExternalEventRecord CreateExternalEventRecord(IEventMessage eventMessage)
        {
            Guid eventId = eventMessage.Metadata.GetEventId() ?? throw new InvalidOperationException($"Cannot queue an external async event ({eventMessage}) without ID");
            (string eventJson, VersionedTypeId typeId) = eventSerializer.SerializeEvent(eventMessage.Event);
            string metadataJson = eventSerializer.SerializeEventMetadata(eventMessage.Metadata);

            return new ExternalEventRecord(eventId, eventJson, typeId.Name, typeId.Version, metadataJson);
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

        private AsyncEventQueueRecordAdapter SelectRecordFromQueuedEvent(QueuedAsyncEvent queuedEvent)
        {
            return new AsyncEventQueueRecordAdapter(queuedEvent, eventSerializer);
        }
    }
}

