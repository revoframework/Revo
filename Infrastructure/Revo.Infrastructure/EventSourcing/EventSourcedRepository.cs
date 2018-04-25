using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStore;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.EventSourcing
{
    internal class EventSourcedRepository<TBase> : IEventSourcedRepository<TBase>,
        IFilteringRepository<IEventSourcedRepository<TBase>>
        where TBase : class, IEventSourcedAggregateRoot
    {
        private readonly Dictionary<Guid, TBase> aggregates = new Dictionary<Guid, TBase>();

        private readonly IEventStore eventStore;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IRepositoryFilter[] repositoryFilters;
        private readonly IEventMessageFactory eventMessageFactory;

        public EventSourcedRepository(IEventStore eventStore,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer,
            IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory,
            IEntityFactory entityFactory)
        {
            this.eventStore = eventStore;
            this.entityTypeManager = entityTypeManager;
            this.publishEventBuffer = publishEventBuffer;
            this.repositoryFilters = repositoryFilters;
            this.eventMessageFactory = eventMessageFactory;
            EntityFactory = entityFactory;
        }

        protected EventSourcedRepository(IEventStore eventStore,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer,
            IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory,
            IEntityFactory entityFactory,
            Dictionary<Guid, TBase> aggregates)
            : this(eventStore, entityTypeManager, publishEventBuffer, repositoryFilters, eventMessageFactory, entityFactory)
        {
            this.aggregates = aggregates;
        }

        public IEnumerable<IRepositoryFilter> DefaultFilters => repositoryFilters;
        public bool IsChanged => aggregates.Values.Any(x => x.IsChanged);

        protected virtual IEntityFactory EntityFactory { get; }
        
        public void Add<T>(T aggregate) where T : class, TBase
        {
            if (aggregates.ContainsKey(aggregate.Id))
            {
                throw new ArgumentException($"Duplicate aggregate root with ID '{aggregate.Id}' and type '{typeof(T).FullName}'");
            }

            aggregates.Add(aggregate.Id, aggregate);
            var classId = entityTypeManager.GetClassIdByClrType(aggregate.GetType());
            eventStore.AddStream(aggregate.Id);
            eventStore.SetStreamMetadataAsync(aggregate.Id,
                new Dictionary<string, string>()
                {
                    {AggregateEventStreamMetadataNames.ClassId, classId.ToString()}
                });
        }

        public T Find<T>(Guid id) where T : class, TBase
        {
            throw new NotImplementedException();
        }

        public TBase Find(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<T> FindAsync<T>(Guid id) where T : class, TBase
        {
            return DoFindAsync<T>(id, false);
        }

        public Task<TBase> FindAsync(Guid id)
        {
            return DoFindAsync<TBase>(id, false);
        }

        public T Get<T>(Guid id) where T : class, TBase
        {
            throw new NotImplementedException();
        }

        public TBase Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(Guid id) where T : class, TBase
        {
            return DoFindAsync<T>(id, true);
        }

        public Task<TBase> GetAsync(Guid id)
        {
            return DoFindAsync<TBase>(id, true);
        }

        public IEnumerable<TBase> GetLoadedAggregates()
        {
            return aggregates.Values;
        }

        public void Remove<T>(T aggregateRoot) where T : class, TBase
        {
            throw new NotImplementedException();
        }

        public virtual IEventSourcedRepository<TBase> IncludeFilters(params IRepositoryFilter[] repositoryFilters)
        {
            return CloneWithFilters(eventStore,
                entityTypeManager,
                publishEventBuffer,
                this.repositoryFilters.Union(repositoryFilters).ToArray(),
                eventMessageFactory,
                aggregates);
        }

        public IEventSourcedRepository<TBase> ExcludeFilter(params IRepositoryFilter[] repositoryFilters)
        {
            return CloneWithFilters(eventStore,
                entityTypeManager,
                publishEventBuffer,
                this.repositoryFilters.Except(repositoryFilters).ToArray(),
                eventMessageFactory,
                aggregates);
        }

        public IEventSourcedRepository<TBase> ExcludeFilters<TRepositoryFilter>() where TRepositoryFilter : IRepositoryFilter
        {
            return CloneWithFilters(eventStore,
                entityTypeManager,
                publishEventBuffer,
                this.repositoryFilters.Where(x => !typeof(TRepositoryFilter).IsAssignableFrom(x.GetType())).ToArray(),
                eventMessageFactory,
                aggregates);
        }
        
        public virtual async Task SaveChangesAsync()
        {
            List<TBase> savedAggregates = aggregates.Values.Where(x => x.IsChanged).ToList();
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

                    IReadOnlyCollection<IEventStoreRecord> eventRecords = await eventStore.PushEventsAsync(aggregate.Id, uncommittedRecords, aggregate.Version);
                    List<IEventMessage> eventMessages = eventRecords.Select(EventStoreEventMessage.FromRecord).ToList();

                    allEventMessages.AddRange(eventMessages);
                }

                await eventStore.CommitChangesAsync();
                allEventMessages.ForEach(publishEventBuffer.PushEvent);
            }

            CommitAggregates();
        }
        
        protected virtual EventSourcedRepository<TBase> CloneWithFilters(
            IEventStore eventStore,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer,
            IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory,
            Dictionary<Guid, TBase> aggregates)
        {
            return new EventSourcedRepository<TBase>(eventStore,
                entityTypeManager,
                publishEventBuffer,
                repositoryFilters,
                eventMessageFactory,
                EntityFactory,
                aggregates);
        }

        protected virtual IEntity ConstructEntity(Type entityType, Guid id)
        {
            return EntityFactory.ConstructEntity(entityType, id);
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
        
        private async Task<List<IEventMessageDraft>> CreateEventMessagesAsync(TBase aggregate, IReadOnlyCollection<DomainAggregateEvent> events)
        {
            var messages = new List<IEventMessageDraft>();
            Guid? aggregateClassId = entityTypeManager.TryGetClassIdByClrType(aggregate.GetType());

            if (aggregateClassId == null)
            {
                throw new InvalidOperationException($"Cannot save event sourced aggregate of type {aggregate.GetType()}: its class ID has not been defined (but will usually be needed for running projections)");
            }

            foreach (DomainAggregateEvent ev in events)
            {
                IEventMessageDraft message = await eventMessageFactory.CreateMessageAsync(ev);
                message.SetMetadata(BasicEventMetadataNames.AggregateClassId, aggregateClassId.Value.ToString());
                messages.Add(message);
            }

            return messages;
        }

        private async Task<T> DoFindAsync<T>(Guid id, bool shouldThrow) where T : class, TBase
        {
            TBase aggregate = FindLoadedAggregate(id);
            if (aggregate == null)
            {
                aggregate = await LoadAggregateAsync(id);
                if (aggregate != null)
                {
                    aggregates.Add(aggregate.Id, aggregate);
                }
            }

            if (aggregate != null)
            {
                aggregate = FilterResult(aggregate);
            }

            if (aggregate == null)
            {
                if (shouldThrow)
                {
                    throw new EntityNotFoundException($"Event sourced aggregate with ID {id} was not found");
                }

                return null;
            }

            if (aggregate.IsDeleted)
            {
                if (shouldThrow)
                {
                    throw new EntityDeletedException(
                        $"Cannot get event sourced {aggregate.GetType().FullName} aggregate with ID {id} because it has been previously deleted");
                }

                return null;
            }

            T typedAggregate = aggregate as T;
            if (typedAggregate == null)
            {
                //always throw
                throw new ArgumentException($"Aggregate root with ID '{id}' is not of requested type '{typeof(T).FullName}'");
            }

            return typedAggregate;
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

        private void FilterDeleted<T>(T deleted) where T : class
        {
            foreach (var repositoryFilter in repositoryFilters)
            {
                repositoryFilter.FilterDeleted(deleted);
            }
        }

        private void FilterModified<T>(T updated) where T : class
        {
            foreach (var repositoryFilter in repositoryFilters)
            {
                repositoryFilter.FilterModified(updated);
            }
        }

        private void CheckEvents(TBase aggregate, IEnumerable<DomainAggregateEvent> uncommitedEvents)
        {
            foreach (DomainAggregateEvent _event in uncommitedEvents)
            {
                if (_event.AggregateId != aggregate.Id)
                {
                    throw new ArgumentException($"Domain aggregate event '{_event.GetType().FullName}' queued for saving has an invalid or empty AggregateId value: {_event.AggregateId}");
                }
            }
        }

        private async Task<TBase> LoadAggregateAsync(Guid aggregateId)
        {
            var eventRecords = await eventStore.GetEventsAsync(aggregateId);
            int version = (int) (eventRecords.LastOrDefault()?.StreamSequenceNumber ?? 0);
            var events = eventRecords.Select(x => x.Event as DomainAggregateEvent
                                                  ?? throw new InvalidOperationException(
                                                      $"Cannot load event sourced aggregate ID {aggregateId}: event stream contains non-DomainAggregateEvent events of type {x.Event.GetType().FullName}"))
                                                      .ToList();

            AggregateState state = new AggregateState(version, events);

            var eventStreamMetadata = await eventStore.GetStreamMetadataAsync(aggregateId);

            if (!eventStreamMetadata.TryGetValue(AggregateEventStreamMetadataNames.ClassId, out string classIdString))
            {
                throw new InvalidOperationException($"Cannot load event sourced aggregate ID {aggregateId}: aggregate class ID not found in event stream metadata");
            }

            Guid classId = Guid.Parse(classIdString);
            Type entityType = entityTypeManager.GetClrTypeByClassId(classId);

            TBase aggregate = (TBase) ConstructEntity(entityType, aggregateId);
            aggregate.LoadState(state);

            return aggregate;
        }

        private TBase FindLoadedAggregate(Guid id, Type entityType = null)
        {
            TBase aggregate;
            if (aggregates.TryGetValue(id, out aggregate))
            {
                if (entityType != null)
                {
                    if (entityType.IsAssignableFrom(aggregate.GetType()))
                    {
                        throw new ArgumentException($"Aggregate root with ID '{id}' is not of requested type '{entityType.FullName}'");
                    }
                }

                return aggregate;
            }
            else
            {
                return null;
            }
        }
    }
}
