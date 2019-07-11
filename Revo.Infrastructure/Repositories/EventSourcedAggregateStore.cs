using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Infrastructure.EventSourcing;

namespace Revo.Infrastructure.Repositories
{
    public class EventSourcedAggregateStore : IAggregateStore
    {
        private readonly IEventSourcedAggregateRepository eventSourcedRepository;

        public EventSourcedAggregateStore(IEventSourcedAggregateRepository eventSourcedRepository)
        {
            this.eventSourcedRepository = eventSourcedRepository;
        }

        public virtual bool NeedsSave => eventSourcedRepository.IsChanged;

        public void Add<T>(T aggregate) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            eventSourcedRepository.Add((IEventSourcedAggregateRoot) aggregate);
        }

        public T Find<T>(Guid id) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            var aggregate = eventSourcedRepository.Find(id);
            return CheckAggregate<T>(id, aggregate, false);
        }

        public async Task<T> FindAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            var aggregate = await eventSourcedRepository.FindAsync(id);
            return CheckAggregate<T>(id, aggregate, false);
        }
        
        public async Task<T[]> FindManyAsync<T>(params Guid[] ids) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            var aggregates = await eventSourcedRepository.FindManyAsync(ids);
            return aggregates.Select(x => CheckAggregate<T>(x.Id, x, false)).ToArray();
        }
        
        public T Get<T>(Guid id) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            var aggregate = eventSourcedRepository.Get(id);
            return CheckAggregate<T>(id, aggregate, false);
        }

        public async Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            var aggregate = await eventSourcedRepository.GetAsync(id);
            return CheckAggregate<T>(id, aggregate, false);
        }

        public async Task<T[]> GetManyAsync<T>(params Guid[] ids) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            var aggregates = await eventSourcedRepository.GetManyAsync(ids);
            return aggregates.Select(x => CheckAggregate<T>(x.Id, x, true)).ToArray();
        }

        public IEnumerable<IAggregateRoot> GetTrackedAggregates()
        {
            return eventSourcedRepository.GetLoadedAggregates();
        }

        public bool CanHandleAggregateType(Type aggregateType)
        {
            return typeof(IEventSourcedAggregateRoot).IsAssignableFrom(aggregateType);
        }

        public virtual Task SaveChangesAsync()
        {
            return eventSourcedRepository.SaveChangesAsync();
        }

        public void Remove<T>(T aggregate) where T : class, IAggregateRoot
        {
            CheckGenericType<T>();
            eventSourcedRepository.Remove((IEventSourcedAggregateRoot)aggregate);
        }

        private void CheckGenericType<T>()
        {
            if (!typeof(IEventSourcedAggregateRoot).IsAssignableFrom(typeof(T)))
            {
                throw new InvalidOperationException($"Cannot use type {typeof(T).FullName} as an aggregate type with {this.GetType().Name} because it does not implement {nameof(IEventSourcedAggregateRoot)}");
            }
        }

        private T CheckAggregate<T>(Guid id, IAggregateRoot aggregate, bool throwOnError) where T : class, IAggregateRoot
        {
            if (aggregate == null)
            {
                return null;
            }

            T typedAggregate = aggregate as T;
            if (typedAggregate == null)
            {
                if (throwOnError)
                {
                    throw new EntityNotFoundException($"Aggregate root with ID '{id}' is not of requested type '{typeof(T).FullName}'");
                }

                return null;
            }

            return typedAggregate;
        }
    }
}
