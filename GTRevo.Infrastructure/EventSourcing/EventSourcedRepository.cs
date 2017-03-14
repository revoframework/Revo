using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Domain;
using GTRevo.Platform.Core;
using GTRevo.Platform.Events;
using GTRevo.Platform.Transactions;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcedRepository : IEventSourcedRepository
    {
        private readonly Dictionary<Guid, IEventSourcedAggregateRoot> aggregates = new Dictionary<Guid, IEventSourcedAggregateRoot>();

        private readonly IEventStore eventStore;
        private readonly IEventQueue eventQueue;
        private readonly IActorContext actorContext;
        private readonly IClock clock;
        private readonly IEntityTypeManager entityTypeManager;

        public EventSourcedRepository(IEventStore eventStore,
            IEventQueue eventQueue,
            IActorContext actorContext,
            IClock clock,
            IEntityTypeManager entityTypeManager)
        {
            this.eventStore = eventStore;
            this.eventQueue = eventQueue;
            this.actorContext = actorContext;
            this.clock = clock;
            this.entityTypeManager = entityTypeManager;
        }

        public ITransaction CreateTransaction()
        {
            return new EventSourcedRepositoryTransaction(this);
        }

        public void Add<T>(T aggregateRoot) where T : class, IEventSourcedAggregateRoot
        {
            if (aggregates.ContainsKey(aggregateRoot.Id))
            {
                throw new ArgumentException($"Duplicate aggregate root with ID '{aggregateRoot.Id}' and type '{typeof(T).FullName}'");
            }

            aggregates.Add(aggregateRoot.Id, aggregateRoot);
            eventStore.AddAggregate(aggregateRoot.Id, aggregateRoot.ClassId);
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

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            bool anyEvents = false;
            foreach (var aggregate in aggregates.Values)
            {
                if (aggregate.UncommitedEvents.Count() > 0)
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

                foreach (var aggregate in aggregates.Values)
                {
                    if (aggregate.UncommitedEvents.Count() > 0)
                    {
                        foreach (DomainAggregateEvent domainEvent in aggregate.UncommitedEvents)
                        {
                            eventQueue.PushEvent(domainEvent);
                        }

                        aggregate.Commit();
                    }
                }
            }
        }

        private List<DomainAggregateEventRecord> ConstructEventsRecords(IEnumerable<DomainAggregateEvent> events,
            int aggregateVersion)
        {
            var now = clock.Now;
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

                if (_event.AggregateClassId != aggregate.ClassId)
                {
                    // NOTE disabled for now because the first *Created event will usually have zero ClassId (virtual getter called from base constructor...)
                    //throw new ArgumentException($"Domain aggregate event '{_event.GetType().FullName}' queued for saving has an invalid or empty AggregateClassId value: {_event.AggregateClassId}");
                }
            }
        }

        private async Task<IEventSourcedAggregateRoot> LoadAggregateAsync(Guid id)
        {
            AggregateState state = await eventStore.GetLastStateAsync(id);

            Guid classId = state.Events.First().AggregateClassId;
            Type entityType = entityTypeManager.GetClrTypeByClassId(classId);

            //TODO: optimize and cache
            ConstructorInfo constructor = entityType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, new Type[] { typeof(Guid), typeof(Guid) }, new ParameterModifier[] {});

            if (constructor == null)
            {
                throw new InvalidOperationException($"Event sourced type {entityType.FullName} does not have a 2-Guid-parameter constructor to be loaded from event repository");
            }

            IEventSourcedAggregateRoot aggregate = (IEventSourcedAggregateRoot)constructor.Invoke(new object[] { id, classId });
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
