using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Domain;
using GTRevo.Infrastructure.Domain.Events;
using GTRevo.Platform.Core;
using GTRevo.Platform.Transactions;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcedRepository : IEventSourcedRepository
    {
        private readonly Dictionary<Guid, IEventSourcedAggregateRoot> aggregates = new Dictionary<Guid, IEventSourcedAggregateRoot>();

        private readonly IEventStore eventStore;
        private readonly IActorContext actorContext;
        private readonly IEntityTypeManager entityTypeManager;

        public EventSourcedRepository(IEventStore eventStore,
            IActorContext actorContext,
            IEntityTypeManager entityTypeManager)
        {
            this.eventStore = eventStore;
            this.actorContext = actorContext;
            this.entityTypeManager = entityTypeManager;
        }

        public ITransaction CreateTransaction()
        {
            return new EventSourcedRepositoryTransaction(this);
        }

        public void Add<T>(T aggregate) where T : class, IEventSourcedAggregateRoot
        {
            if (aggregates.ContainsKey(aggregate.Id))
            {
                throw new ArgumentException($"Duplicate aggregate root with ID '{aggregate.Id}' and type '{typeof(T).FullName}'");
            }

            aggregates.Add(aggregate.Id, aggregate);
            var classId = entityTypeManager.GetClassIdByClrType(aggregate.GetType());
            eventStore.AddAggregate(aggregate.Id, classId);
        }

        public T Get<T>(Guid id) where T : class, IEventSourcedAggregateRoot
        {
            throw new NotImplementedException();
        }

        public IEventSourcedAggregateRoot Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetAsync<T>(Guid id) where T : class, IEventSourcedAggregateRoot
        {
            var aggregate = await GetAsync(id);
            T typedAggregate = aggregate as T;

            if (typedAggregate == null)
            {
                throw new ArgumentException($"Aggregate root with ID '{id}' is not of requested type '{typeof(T).FullName}'");
            }

            return typedAggregate;
        }

        public async Task<IEventSourcedAggregateRoot> GetAsync(Guid id)
        {
            IEventSourcedAggregateRoot aggregate = FindLoadedAggregate(id);
            if (aggregate != null)
            {
                return aggregate;
            }

            aggregate = await LoadAggregateAsync(id);
            aggregates.Add(aggregate.Id, aggregate);
            return aggregate;
        }

        public IEnumerable<IAggregateRoot> GetLoadedAggregates()
        {
            return aggregates.Values;
        }

        public void Remove<T>(T aggregateRoot) where T : class, IEventSourcedAggregateRoot
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            bool anyEvents = false;
            foreach (var aggregate in aggregates.Values)
            {
                if (aggregate.UncommitedEvents.Any())
                {
                    int newAggregateVersion = aggregate.Version + 1;
                    var eventRecords = ConstructEventsRecords(aggregate.UncommitedEvents, newAggregateVersion);

                    CheckEvents(aggregate, aggregate.UncommitedEvents);
                    await eventStore.PushEventsAsync(aggregate.Id, eventRecords, newAggregateVersion);
                    anyEvents = true;
                }
            }

            if (anyEvents)
            {
                await eventStore.CommitChangesAsync();
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

        private void CheckEvents(IEventSourcedAggregateRoot aggregate, IEnumerable<DomainAggregateEvent> uncommitedEvents)
        {
            foreach (DomainAggregateEvent _event in uncommitedEvents)
            {
                if (_event.AggregateId != aggregate.Id)
                {
                    throw new ArgumentException($"Domain aggregate event '{_event.GetType().FullName}' queued for saving has an invalid or empty AggregateId value: {_event.AggregateId}");
                }

                _event.AggregateClassId = entityTypeManager.GetClassIdByClrType(aggregate.GetType());
            }
        }

        private async Task<IEventSourcedAggregateRoot> LoadAggregateAsync(Guid id)
        {
            AggregateState state = await eventStore.GetLastStateAsync(id);

            Guid classId = state.Events.First().AggregateClassId;
            Type entityType = entityTypeManager.GetClrTypeByClassId(classId);

            //TODO: optimize and cache
            ConstructorInfo constructor = entityType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, new Type[] { typeof(Guid) }, new ParameterModifier[] {});

            if (constructor == null)
            {
                throw new InvalidOperationException($"Event sourced entity of type {entityType.FullName} does not have a constructor with 1 Guid parameters");
            }

            IEventSourcedAggregateRoot aggregate = (IEventSourcedAggregateRoot)constructor.Invoke(new object[] { id });
            aggregate.LoadState(state);

            return aggregate;
        }

        private IEventSourcedAggregateRoot FindLoadedAggregate(Guid id, Type entityType = null)
        {
            IEventSourcedAggregateRoot aggregate;
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
