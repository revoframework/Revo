using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using MoreLinq;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcedRepository<TBase> : IEventSourcedRepository<TBase>,
        IFilteringRepository<IEventSourcedRepository<TBase>>
        where TBase : class, IEventSourcedAggregateRoot
    {
        private readonly Dictionary<Guid, TBase> aggregates = new Dictionary<Guid, TBase>();

        private readonly IEventStore eventStore;
        private readonly IActorContext actorContext;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IEventQueue eventQueue;
        private readonly IRepositoryFilter[] repositoryFilters;

        public EventSourcedRepository(IEventStore eventStore,
            IActorContext actorContext,
            IEntityTypeManager entityTypeManager,
            IEventQueue eventQueue,
            IRepositoryFilter[] repositoryFilters)
        {
            this.eventStore = eventStore;
            this.actorContext = actorContext;
            this.entityTypeManager = entityTypeManager;
            this.eventQueue = eventQueue;
            this.repositoryFilters = repositoryFilters;
        }

        private EventSourcedRepository(IEventStore eventStore,
            IActorContext actorContext,
            IEntityTypeManager entityTypeManager,
            IEventQueue eventQueue,
            IRepositoryFilter[] repositoryFilters,
            Dictionary<Guid, TBase> aggregates)
            : this(eventStore, actorContext, entityTypeManager, eventQueue, repositoryFilters)
        {
            this.aggregates = aggregates;
        }

        public IEnumerable<IRepositoryFilter> DefaultFilters => repositoryFilters;

        protected virtual IEventSourcedEntityFactory EntityFactory { get; } = new EventSourcedEntityFactory();

        public ITransaction CreateTransaction()
        {
            return new EventSourcedRepositoryTransaction<TBase>(this);
        }

        public void Add<T>(T aggregate) where T : class, TBase
        {
            if (aggregates.ContainsKey(aggregate.Id))
            {
                throw new ArgumentException($"Duplicate aggregate root with ID '{aggregate.Id}' and type '{typeof(T).FullName}'");
            }

            aggregates.Add(aggregate.Id, aggregate);
            var classId = entityTypeManager.GetClassIdByClrType(aggregate.GetType());
            eventStore.AddAggregate(aggregate.Id, classId);
        }

        public T Get<T>(Guid id) where T : class, TBase
        {
            throw new NotImplementedException();
        }

        public TBase Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetAsync<T>(Guid id) where T : class, TBase
        {
            var aggregate = await GetAsync(id);
            T typedAggregate = aggregate as T;

            if (typedAggregate == null)
            {
                throw new ArgumentException($"Aggregate root with ID '{id}' is not of requested type '{typeof(T).FullName}'");
            }

            return typedAggregate;
        }

        public async Task<TBase> GetAsync(Guid id)
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
                throw new EntityNotFoundException($"Event sourced aggregate with ID {id} was not found");
            }

            if (aggregate.IsDeleted)
            {
                throw new EntityDeletedException(
                    $"Cannot get event sourced {aggregate.GetType().FullName} aggregate with ID {id} because it has been previously deleted");
            }

            return aggregate;
        }

        public IEnumerable<TBase> GetLoadedAggregates()
        {
            return aggregates.Values;
        }

        public void Remove<T>(T aggregateRoot) where T : class, TBase
        {
            throw new NotImplementedException();
        }

        public IEventSourcedRepository<TBase> IncludeFilters(params IRepositoryFilter[] repositoryFilters)
        {
            return new EventSourcedRepository<TBase>(eventStore, actorContext, entityTypeManager,
                eventQueue,
                this.repositoryFilters.Union(repositoryFilters).ToArray(),
                aggregates);
        }

        public IEventSourcedRepository<TBase> ExcludeFilter(params IRepositoryFilter[] repositoryFilters)
        {
            return new EventSourcedRepository<TBase>(eventStore, actorContext, entityTypeManager,
                eventQueue,
                this.repositoryFilters.Except(repositoryFilters).ToArray(),
                aggregates);
        }

        public IEventSourcedRepository<TBase> ExcludeFilters<TRepositoryFilter>() where TRepositoryFilter : IRepositoryFilter
        {
            return new EventSourcedRepository<TBase>(eventStore, actorContext, entityTypeManager,
                eventQueue,
                this.repositoryFilters.Where(x => !typeof(TRepositoryFilter).IsAssignableFrom(x.GetType())).ToArray(),
                aggregates);
        }

        public virtual void SaveChanges()
        {
            List<TBase> savedAggregates = aggregates.Values.Where(x => x.UncommittedEvents.Any()).ToList();
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

                foreach (var aggregate in savedAggregates)
                {
                    int newAggregateVersion = aggregate.Version + 1;
                    var eventRecords = ConstructEventsRecords(aggregate.UncommittedEvents, newAggregateVersion);
                    eventStore.PushEvents(aggregate.Id, eventRecords, newAggregateVersion);
                }

                eventStore.CommitChanges();
            }

            CommitAggregates();
        }

        public virtual async Task SaveChangesAsync()
        {
            List<TBase> savedAggregates = aggregates.Values.Where(x => x.UncommittedEvents.Any()).ToList();
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

                foreach (var aggregate in savedAggregates)
                {
                    int newAggregateVersion = aggregate.Version + 1;
                    var eventRecords = ConstructEventsRecords(aggregate.UncommittedEvents, newAggregateVersion);
                    await eventStore.PushEventsAsync(aggregate.Id, eventRecords, newAggregateVersion);
                }

                await eventStore.CommitChangesAsync();
            }

            CommitAggregates();
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
                    aggregate.UncommittedEvents.ForEach(eventQueue.PushEvent);
                    aggregate.Commit();
                }
            }
        }

        private List<DomainAggregateEventRecord> ConstructEventsRecords(IEnumerable<DomainAggregateEvent> events,
            int aggregateVersion)
        {
            var now = Clock.Current.Now;
            var records = new List<DomainAggregateEventRecord>();

            foreach (DomainAggregateEvent ev in events)
            {
                records.Add(new DomainAggregateEventRecord()
                {
                    Event = ev,
                    ActorName = actorContext.CurrentActorName,
                    AggregateVersion = aggregateVersion,
                    DatePublished = now
                });
            }

            return records;
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
            Guid aggregateClassId = entityTypeManager.GetClassIdByClrType(aggregate.GetType());

            foreach (DomainAggregateEvent _event in uncommitedEvents)
            {
                if (_event.AggregateId != aggregate.Id)
                {
                    throw new ArgumentException($"Domain aggregate event '{_event.GetType().FullName}' queued for saving has an invalid or empty AggregateId value: {_event.AggregateId}");
                }

                _event.AggregateClassId = aggregateClassId;
            }
        }

        private async Task<TBase> LoadAggregateAsync(Guid id)
        {
            AggregateState state = await eventStore.GetLastStateAsync(id);

            Guid classId = state.Events.First().AggregateClassId;
            Type entityType = entityTypeManager.GetClrTypeByClassId(classId);

            TBase aggregate = (TBase)ConstructEntity(entityType, id);
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
