using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.EventSourcing;

namespace GTRevo.Infrastructure.Repositories
{
    public class EventSourcedAggregateStore : IAggregateStore
    {
        private readonly IEventSourcedAggregateRepository eventSourcedRepository;

        public EventSourcedAggregateStore(IEventSourcedAggregateRepository eventSourcedRepository)
        {
            this.eventSourcedRepository = eventSourcedRepository;
        }

        public void Add<T>(T aggregate) where T : class, IAggregateRoot
        {
            eventSourcedRepository.Add((dynamic)aggregate);
        }

        public T Get<T>(Guid id) where T : class, IAggregateRoot
        {
            return ((dynamic)eventSourcedRepository).Get<T>(id);
        }

        public Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return ((dynamic)eventSourcedRepository).GetAsync<T>(id);
        }

        public IEnumerable<IAggregateRoot> GetTrackedAggregates()
        {
            return eventSourcedRepository.GetLoadedAggregates();
        }

        public bool CanHandleAggregateType(Type aggregateType)
        {
            return typeof(IEventSourcedAggregateRoot).IsAssignableFrom(aggregateType);
        }

        public void SaveChanges()
        {
            eventSourcedRepository.SaveChanges();
        }

        public Task SaveChangesAsync()
        {
            return eventSourcedRepository.SaveChangesAsync();
        }

        public void Remove<T>(T aggregate) where T : class, IAggregateRoot
        {
            eventSourcedRepository.Remove((dynamic)aggregate);
        }
    }
}
