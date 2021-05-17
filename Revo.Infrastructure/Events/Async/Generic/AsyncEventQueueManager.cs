using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.EventStores.Generic;
using EntityState = Revo.DataAccess.Entities.EntityState;

namespace Revo.Infrastructure.Events.Async.Generic
{
    public class AsyncEventQueueManager : IAsyncEventQueueManager
    {
        private readonly ICrudRepository crudRepository;
        private readonly IQueuedAsyncEventMessageFactory queuedAsyncEventMessageFactory;
        private readonly IExternalEventStore externalEventStore;

        private readonly Dictionary<IEventMessage, List<EventSequencing>> eventsToQueues
            = new Dictionary<IEventMessage, List<EventSequencing>>();

        public AsyncEventQueueManager(ICrudRepository crudRepository, IQueuedAsyncEventMessageFactory queuedAsyncEventMessageFactory,
            IExternalEventStore externalEventStore)
        {
            this.crudRepository = crudRepository;
            this.queuedAsyncEventMessageFactory = queuedAsyncEventMessageFactory;
            this.externalEventStore = externalEventStore;
        }

        public async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> FindQueuedEventsAsync(Guid[] asyncEventQueueRecordIds)
        {
            return (await QueryQueuedEvents()
                .Where(x => asyncEventQueueRecordIds.Contains(x.Id))
                .ToArrayAsync(crudRepository))
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
            var queue = await FindQueueAsync(queueName);
            return queue; //can be null
        }

        public async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> GetQueueEventsAsync(string queueName)
        {
            var queue = await FindQueueAsync(queueName);
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
        
        public Task EnqueueEventAsync(IEventMessage eventMessage, IReadOnlyCollection<EventSequencing> queues)
        {
            if (!eventsToQueues.TryGetValue(eventMessage, out var queueList)
                || (queueList == null && queues.Count > 0))
            {
                queueList = eventsToQueues[eventMessage] = queues.Count > 0 ? new List<EventSequencing>() : null;
            }

            if (queues.Count > 0)
            {
                // ReSharper disable once PossibleNullReferenceException
                queueList.AddRange(queues);
            }

            return Task.CompletedTask;
        }

        public async Task<string> GetEventSourceCheckpointAsync(string eventSourceName)
        {
            var eventSourceState = await crudRepository.FindAsync<EventSourceState>(eventSourceName);
            return eventSourceState?.EventEnqueueCheckpoint;
        }

        public async Task SetEventSourceCheckpointAsync(string eventSourceName, string opaqueCheckpoint)
        {
            var eventSourceState = await crudRepository.FindAsync<EventSourceState>(eventSourceName);
            if (eventSourceState == null)
            {
                eventSourceState = new EventSourceState(eventSourceName);
                crudRepository.Add(eventSourceState);
            }

            eventSourceState.EventEnqueueCheckpoint = opaqueCheckpoint;
        }

        public virtual async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> CommitAsync()
        {
            var queuedEvents = await EnqueueEventsToRepositoryAsync();
            await crudRepository.SaveChangesAsync();
            return queuedEvents.Select(SelectRecordFromQueuedEvent).ToList();
        }

        protected async Task<List<QueuedAsyncEvent>> EnqueueEventsToRepositoryAsync()
        {
            var queuedEvents = new List<QueuedAsyncEvent>();

            EnqueueEventStoreEvents(queuedEvents);
            await EnqueueExternalEventsAsync(queuedEvents);
            await CreateQueuesAsync(queuedEvents);
            
            return queuedEvents;
        }

        private async Task CreateQueuesAsync(List<QueuedAsyncEvent> queuedEvents)
        {
            var queueNames = queuedEvents.Select(x => x.QueueId).Distinct().ToArray();
            var existingQueues = await crudRepository.FindManyAsync<AsyncEventQueue, string>(queueNames);
            foreach (var queueName in queueNames)
            {
                var queue = existingQueues.FirstOrDefault(x => x.Id == queueName);
                if (queue == null)
                {
                    queue = new AsyncEventQueue(queueName, null);
                    crudRepository.Add(queue);
                }
            }
        }

        private void EnqueueEventStoreEvents(List<QueuedAsyncEvent> queuedEvents)
        {
            // all event store events need to be marked as dispatched (even whey they haven't been dispatched to any queues)
            var eventStoreEvents = eventsToQueues
                .Where(x => (x.Key as IEventStoreEventMessage)?.Record is IEventStreamRowEventStoreRecord)
                .ToArray();
            
            foreach (var eventToQueues in eventStoreEvents)
            {
                var storeMessage = (IEventStoreEventMessage)eventToQueues.Key;
                var eventStreamRow = ((IEventStreamRowEventStoreRecord)storeMessage.Record).EventStreamRow;
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

                var eventSequencings = eventToQueues.Value;
                if (eventSequencings != null)
                {
                    foreach (var sequencing in eventSequencings)
                    {
                        var queuedEvent = new QueuedAsyncEvent(Guid.NewGuid(), sequencing.SequenceName,
                            eventStreamRow, sequencing.EventSequenceNumber);
                        crudRepository.Add(queuedEvent);
                        queuedEvents.Add(queuedEvent);
                    }
                }
            }
            
            eventStoreEvents.ForEach(x => eventsToQueues.Remove(x.Key));
        }

        private async Task EnqueueExternalEventsAsync(List<QueuedAsyncEvent> queuedEvents)
        {
            // external events are only stored if they match any queues
            var externalEvents = eventsToQueues
                .Where(x => !(x.Key is IEventStoreEventMessage eventStoreMessage)
                            || !(eventStoreMessage.Record is IEventStreamRowEventStoreRecord))
                .Where(x => x.Value?.Count > 0)
                .Select(x =>
                {
                    // clone and generate event IDs where missing
                    var eventId = x.Key.Metadata.GetEventId();
                    if (eventId == null)
                    {
                        return new
                        {
                            OriginalMessage = x.Key,
                            Message = CloneMessageWithId(x.Key),
                            EventSequencings = x.Value
                        };
                    }

                    return new
                    {
                        OriginalMessage = x.Key,
                        Message = x.Key,
                        EventSequencings = x.Value
                    };
                })
                .ToArray();
            
            if (externalEvents.Length > 0)
            {
                foreach (var externalEvent in externalEvents)
                {
                    externalEventStore.TryPushEvent(externalEvent.Message);
                    eventsToQueues.Remove(externalEvent.OriginalMessage); // we need to remove the events before the CommitAsync below, which might trigger another execution of this method
                }

                var externalEventRecords = await externalEventStore.CommitAsync();
                foreach (var externalEventRecord in externalEventRecords)
                {
                    if (externalEventRecord.IsDispatchedToAsyncQueues)
                    {
                        continue;
                    }

                    externalEventRecord.MarkDispatchedToAsyncQueues();

                    if (!crudRepository.IsAttached(externalEventRecord))
                    {
                        crudRepository.Attach(externalEventRecord);
                        crudRepository.SetEntityState(externalEventRecord, EntityState.Modified);
                    }

                    var sequencings = externalEvents.FirstOrDefault(x =>
                        x.Message.Metadata.GetEventId() == externalEventRecord.Id)?.EventSequencings;
                    if (sequencings == null) // may happen with providers like EF Core with coordinated transactions
                    {
                        continue;
                    }

                    foreach (var sequencing in sequencings)
                    {
                        var queuedEvent = new QueuedAsyncEvent(Guid.NewGuid(), sequencing.SequenceName,
                            externalEventRecord, sequencing.EventSequenceNumber);
                        crudRepository.Add(queuedEvent);
                        queuedEvents.Add(queuedEvent);
                    }
                }
            }
        }

        private Task<AsyncEventQueue> FindQueueAsync(string queueName)
        {
            return crudRepository.FindAsync<AsyncEventQueue>(queueName);
        }
        
        private IQueryable<QueuedAsyncEvent> QueryQueuedEvents()
        {
            return crudRepository
                .FindAll<QueuedAsyncEvent>()
                .OrderBy(x => x.SequenceNumber)
                .Include(crudRepository, x => x.EventStreamRow)
                .Include(crudRepository, x => x.ExternalEventRecord);
        }

        private async Task<AsyncEventQueue> GetOrCreateQueueAsync(string queueName, long? lastSequenceNumberProcessed)
        {
            AsyncEventQueue queue = await FindQueueAsync(queueName);
            if (queue == null)
            {
                queue = new AsyncEventQueue(queueName, lastSequenceNumberProcessed);
                crudRepository.Add(queue);
            }

            return queue;
        }

        protected AsyncEventQueueRecordAdapter SelectRecordFromQueuedEvent(QueuedAsyncEvent queuedEvent)
        {
            return new AsyncEventQueueRecordAdapter(queuedEvent, queuedAsyncEventMessageFactory);
        }

        private IEventMessage CloneMessageWithId(IEventMessage eventMessage)
        {
            return eventMessage
                .Clone()
                .SetId(Guid.NewGuid());
        }
    }
}

