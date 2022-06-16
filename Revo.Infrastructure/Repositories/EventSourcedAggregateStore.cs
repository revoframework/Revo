using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Domain.Tenancy;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.EventStores;

namespace Revo.Infrastructure.Repositories
{
    public class EventSourcedAggregateStore : IAggregateStore
    {
        private readonly Dictionary<Guid, IEventSourcedAggregateRoot> aggregates = new Dictionary<Guid, IEventSourcedAggregateRoot>();

        private readonly IEventStore eventStore;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IRepositoryFilter[] repositoryFilters;
        private readonly IEventMessageFactory eventMessageFactory;
        private readonly IEventSourcedAggregateFactory eventSourcedAggregateFactory;

        public EventSourcedAggregateStore(IEventStore eventStore, IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer, IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory, IEventSourcedAggregateFactory eventSourcedAggregateFactory)
        {
            this.eventStore = eventStore;
            this.entityTypeManager = entityTypeManager;
            this.publishEventBuffer = publishEventBuffer;
            this.repositoryFilters = repositoryFilters;
            this.eventMessageFactory = eventMessageFactory;
            this.eventSourcedAggregateFactory = eventSourcedAggregateFactory;
        }

        public IReadOnlyCollection<IRepositoryFilter> DefaultFilters => repositoryFilters;
        public virtual bool NeedsSave => aggregates.Values.Any(x => x.IsChanged);

        public void Add<T>(T aggregate) where T : class, IAggregateRoot
        {
            var esAggregate = GetEventSourcedAggregate(aggregate);

            if (aggregates.TryGetValue(aggregate.Id, out var existing))
            {
                if (existing == aggregate)
                {
                    return;
                }

                throw new ArgumentException($"Duplicate aggregate root with ID '{aggregate.Id}' and type '{typeof(T).FullName}'");
            }

            aggregates.Add(aggregate.Id, esAggregate);
            Guid classId = entityTypeManager.GetClassInfoByClrType(aggregate.GetType()).Id;
            eventStore.AddStream(aggregate.Id);
            eventStore.SetStreamMetadata(aggregate.Id,
                new Dictionary<string, string>()
                {
                    {AggregateEventStreamMetadataNames.ClassId, classId.ToString()}
                });
        }

        public async Task<T> FindAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            var aggregate = await DoFindAsync<T>(id, false);
            return aggregate;
        }
        
        public async Task<T[]> FindManyAsync<T>(params Guid[] ids) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            var aggregates = await DoFindManyAsync<T>(ids, false);
            return aggregates;
        }
        
        public async Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            var aggregate = await DoFindAsync<T>(id, true);
            return aggregate;
        }

        public async Task<T[]> GetManyAsync<T>(params Guid[] ids) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            var aggregates = await DoFindManyAsync<T>(ids, true);
            return aggregates;
        }

        public IEnumerable<IAggregateRoot> GetTrackedAggregates()
        {
            return aggregates.Values;
        }

        public bool CanHandleAggregateType(Type aggregateType)
        {
            return typeof(IEventSourcedAggregateRoot).IsAssignableFrom(aggregateType);
        }

        public virtual async Task SaveChangesAsync()
        {
            List<IEventSourcedAggregateRoot> savedAggregates = aggregates.Values.Where(x => x.IsChanged).ToList();
            if (savedAggregates.Any())
            {
                foreach (var aggregate in savedAggregates)
                {
                    CheckEvents(aggregate, aggregate.UncommittedEvents);

                    if (aggregate.Version == 0)
                    {
                        FilterAdded(aggregate);
                    }
                    else
                    {
                        FilterModified(aggregate);
                    }
                }

                List<IEventMessage> allEventMessages = new List<IEventMessage>();

                foreach (var aggregate in savedAggregates)
                {
                    List<IEventMessageDraft> eventMessageDrafts = await CreateEventMessagesAsync(aggregate, aggregate.UncommittedEvents);
                    List<UncommitedEventStoreRecord> uncommittedRecords = eventMessageDrafts.Select(x => new UncommitedEventStoreRecord(x.Event, x.Metadata)).ToList();

                    IReadOnlyCollection<IEventStoreRecord> eventRecords = await eventStore.PushEventsAsync(aggregate.Id, uncommittedRecords);
                    List<IEventMessage> eventMessages = eventRecords.Select(EventStoreEventMessage.FromRecord).ToList();

                    allEventMessages.AddRange(eventMessages);
                }

                await eventStore.CommitChangesAsync();

                if (publishEventBuffer != null)
                {
                    allEventMessages.ForEach(publishEventBuffer.PushEvent);
                }
            }

            CommitAggregates();
        }

        public void Remove<T>(T aggregate) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            throw new NotSupportedException("Explicit removal of event-sourced aggregates is not supported - instead, publish an event that marks the aggregate root as removed using MarkDeleted");
        }

        private void CheckGenericType<T>()
        {
            if (!CanHandleAggregateType(typeof(T)))
            {
                throw new InvalidOperationException($"Cannot use {typeof(T)} with {nameof(EventSourcedAggregateStore)}");
            }
        }

        private IEventSourcedAggregateRoot GetEventSourcedAggregate(IAggregateRoot aggregate)
        {
            return aggregate as IEventSourcedAggregateRoot
                   ?? throw new InvalidOperationException(
                       $"{aggregate} does not implement {nameof(IEventSourcedAggregateRoot)}");
        }

        private T CheckAggregate<T>(Guid id, IAggregateRoot aggregate, bool throwOnError) where T : class, IAggregateRoot
        {
            if (aggregate != null)
            {
                aggregate = FilterResult(aggregate);
            }

            if (aggregate == null)
            {
                if (throwOnError)
                {
                    throw new EntityNotFoundException($"Event sourced aggregate with ID {id} was not found");
                }

                return null;
            }

            if (aggregate.IsDeleted)
            {
                if (throwOnError)
                {
                    throw new EntityDeletedException(
                        $"Cannot get event sourced {aggregate.GetType().FullName} aggregate with ID {id} because it has been previously deleted");
                }

                return null;
            }

            T typedAggregate = aggregate as T;
            if (typedAggregate == null)
            {
                if (throwOnError)
                {
                    throw new EntityNotFoundException($"{aggregate} is not of requested type {typeof(T).FullName}");
                }

                return null;
            }

            return typedAggregate;
        }

        protected virtual void CommitAggregates()
        {
            foreach (var aggregate in aggregates.Values)
            {
                if (aggregate.IsChanged)
                {
                    aggregate.Commit();
                }
            }
        }

        private async Task<List<IEventMessageDraft>> CreateEventMessagesAsync(IEventSourcedAggregateRoot aggregate, IReadOnlyCollection<DomainAggregateEvent> events)
        {
            var messages = new List<IEventMessageDraft>();
            Guid? aggregateClassId = entityTypeManager.TryGetClassInfoByClrType(aggregate.GetType())?.Id;

            if (aggregateClassId == null)
            {
                throw new InvalidOperationException($"Cannot save event sourced aggregate of type {aggregate.GetType()}: its class ID has not been defined");
            }

            foreach (DomainAggregateEvent ev in events)
            {
                IEventMessageDraft message = await eventMessageFactory.CreateMessageAsync(ev);
                message.SetMetadata(BasicEventMetadataNames.AggregateClassId, aggregateClassId.Value.ToString());
                message.SetMetadata(BasicEventMetadataNames.AggregateVersion, (aggregate.Version + 1).ToString());

                if (aggregate is ITenantOwned tenantOwned)
                {
                    message.SetMetadata(BasicEventMetadataNames.AggregateTenantId, tenantOwned.TenantId?.ToString());
                }

                messages.Add(message);
            }

            return messages;
        }

        private async Task<T> DoFindAsync<T>(Guid id, bool throwOnError) where T : class, IAggregateRoot
        {
            IEventSourcedAggregateRoot aggregate = FindLoadedAggregate(id);
            if (aggregate == null)
            {
                aggregate = await LoadAggregateAsync(id);
                if (aggregate != null)
                {
                    aggregates.Add(aggregate.Id, aggregate);
                }
            }

            return CheckAggregate<T>(id, aggregate, throwOnError);
        }

        private async Task<T[]> DoFindManyAsync<T>(Guid[] ids, bool throwOnError) where T : class, IAggregateRoot
        {
            var result = new List<T>();
            List<Guid> missingIds = null;
            foreach (Guid id in ids)
            {
                IEventSourcedAggregateRoot aggregate = FindLoadedAggregate(id);
                if (aggregate != null)
                {
                    var typedAggregate = CheckAggregate<T>(id, aggregate, throwOnError);
                    result.Add(typedAggregate);
                }
                else
                {
                    if (missingIds == null)
                    {
                        missingIds = new List<Guid>();
                    }

                    missingIds.Add(id);
                }
            }

            if (missingIds?.Count > 0)
            {
                var loaded = await LoadAggregatesAsync(missingIds.ToArray());

                if (throwOnError)
                {
                    var notFoundIds = missingIds.Where(x => !loaded.ContainsKey(x)).ToArray();
                    if (notFoundIds.Length > 0)
                    {
                        throw new EntityNotFoundException($"Aggregate(s) of type {typeof(T)} with ID(s) {string.Join(", ", notFoundIds)} were not found");
                    }
                }

                foreach (var aggregatePair in loaded)
                {
                    var typedAggregate = CheckAggregate<T>(aggregatePair.Key, aggregatePair.Value, throwOnError);
                    if (typedAggregate != null)
                    {
                        result.Add(typedAggregate);
                        aggregates.Add(aggregatePair.Key, aggregatePair.Value);
                    }
                }
            }

            return result.ToArray();
        }

        private T FilterResult<T>(T result) where T : class
        {
            if (result == null)
            {
                return null;
            }

            T intermed = result;
            foreach (var repositoryFilter in repositoryFilters)
            {
                if (intermed == null)
                {
                    break;
                }

                intermed = repositoryFilter.FilterResult(intermed);
            }

            return intermed;
        }

        private void FilterAdded<T>(T inserted) where T : class
        {
            foreach (var repositoryFilter in repositoryFilters)
            {
                repositoryFilter.FilterAdded(inserted);
            }
        }
        
        private void FilterModified<T>(T updated) where T : class
        {
            foreach (var repositoryFilter in repositoryFilters)
            {
                repositoryFilter.FilterModified(updated);
            }
        }

        private void CheckEvents(IEventSourcedAggregateRoot aggregate, IEnumerable<DomainAggregateEvent> uncommitedEvents)
        {
            foreach (DomainAggregateEvent _event in uncommitedEvents)
            {
                if (_event.AggregateId != aggregate.Id)
                {
                    throw new ArgumentException($"Domain aggregate event '{_event.GetType().FullName}' queued for saving has an invalid or empty AggregateId value: {_event.AggregateId}");
                }
            }
        }

        private async Task<IEventSourcedAggregateRoot> LoadAggregateAsync(Guid aggregateId)
        {
            IReadOnlyDictionary<string, string> eventStreamMetadata = eventStreamMetadata = await eventStore.FindStreamMetadataAsync(aggregateId);
            if (eventStreamMetadata == null)
            {
                return null;
            }

            IReadOnlyCollection<IEventStoreRecord> eventRecords = await eventStore.FindEventsAsync(aggregateId);

            return eventSourcedAggregateFactory.ConstructAndLoadEntityFromEvents(aggregateId, eventStreamMetadata,
                eventRecords?.Count > 0 ? eventRecords : new IEventStoreRecord[0]);
        }

        private async Task<Dictionary<Guid, IEventSourcedAggregateRoot>> LoadAggregatesAsync(Guid[] aggregateIds)
        {
            IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, string>> eventStreamMetadata = await eventStore.BatchFindStreamMetadataAsync(aggregateIds);
            IDictionary<Guid, IReadOnlyCollection<IEventStoreRecord>> eventRecords = await eventStore.BatchFindEventsAsync(aggregateIds);
            return eventStreamMetadata.ToDictionary(x => x.Key,
                x =>
                {
                    eventRecords.TryGetValue(x.Key, out var events);
                    return eventSourcedAggregateFactory.ConstructAndLoadEntityFromEvents(x.Key, x.Value,
                        events?.Count > 0 ? events : new IEventStoreRecord[0]);
                });
        }


        private IEventSourcedAggregateRoot FindLoadedAggregate(Guid id)
        {
            IEventSourcedAggregateRoot aggregate;
            aggregates.TryGetValue(id, out aggregate);
            return aggregate;
        }
    }
}
