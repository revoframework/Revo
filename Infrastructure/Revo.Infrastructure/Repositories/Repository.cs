using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Entities;

namespace Revo.Infrastructure.Repositories
{
    public class Repository : IRepository
    {
        private readonly IAggregateStore[] aggregateStores;
        private readonly IPublishEventBuffer eventQueue; 

        public Repository(IAggregateStore[] aggregateStores, IPublishEventBuffer eventQueue)
        {
            this.aggregateStores = aggregateStores;
            this.eventQueue = eventQueue;
        }

        public IAggregateStore GetAggregateStore(Type entityType)
        {
            return aggregateStores.First(x => x.CanHandleAggregateType(entityType));
        }

        public IAggregateStore GetAggregateStore<T>()
        {
            return GetAggregateStore(typeof(T));
        }

        public IQueryableAggregateStore GetQueyrableAggregateStore<T>()
        {
            var aggregateStore = GetAggregateStore(typeof(T));
            var queryableAggregateStore = aggregateStore as IQueryableAggregateStore;

            if (queryableAggregateStore == null)
            {
                throw new InvalidOperationException(
                    $"Aggregate store {aggregateStore.GetType().FullName} for entity type {typeof(T).FullName} does not implement queryable IQueryableAggregateStore");
            }

            return queryableAggregateStore;
        }

        public void Add<T>(T aggregate) where T : class, IAggregateRoot
        {
            /*if (aggregates.ContainsKey(aggregate.Id))
            {
                throw new ArgumentException($"Duplicate aggregate root with ID '{aggregate.Id}' and type '{typeof(T).FullName}'");
            }*/

            var aggregateStore = GetAggregateStore<T>();
            aggregateStore.Add(aggregate);
            //AddTrackedAggregate(aggregate, aggregateStore);
        }

        public ITransaction CreateTransaction()
        {
            return new RepositoryTransaction(this);
        }

        public void Dispose()
        {
            // TODO ?
        }

        public void SaveChanges()
        {
            foreach (var aggregateStore in aggregateStores)
            {
                aggregateStore.SaveChanges();
            }
        }

        public async Task SaveChangesAsync()
        {
            foreach (var aggregateStore in aggregateStores)
            {
                await aggregateStore.SaveChangesAsync();
            }
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            T aggregate = aggregateStore.FirstOrDefault(predicate);
            //AddTrackedAggregate(aggregate, aggregateStore);
            return aggregate;
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            T aggregate = aggregateStore.First(predicate);
            //AddTrackedAggregate(aggregate, aggregateStore);
            return aggregate;
        }

        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            T aggregate = await aggregateStore.FirstOrDefaultAsync(predicate);
            //AddTrackedAggregate(aggregate, aggregateStore);
            return aggregate;
        }

        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            T aggregate = await aggregateStore.FirstAsync(predicate);
            //AddTrackedAggregate(aggregate, aggregateStore);
            return aggregate;
        }

        public T Find<T>(Guid id) where T : class, IAggregateRoot
        {
            var aggregateStore = GetAggregateStore<T>();
            T aggregate = aggregateStore.Find<T>(id);
            //AddTrackedAggregate(aggregate, aggregateStore);
            return aggregate;
        }

        public async Task<T> FindAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            var aggregateStore = GetAggregateStore<T>();
            T aggregate = await aggregateStore.FindAsync<T>(id);
            //AddTrackedAggregate(aggregate, aggregateStore);
            return aggregate;
        }

        public IQueryable<T> FindAll<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            return aggregateStore.FindAll<T>();
        }

        public Task<IList<T>> FindAllAsync<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            return aggregateStore.FindAllAsync<T>();
        }

        public T Get<T>(Guid id) where T : class, IAggregateRoot
        {
            var aggregateStore = GetAggregateStore<T>();
            T aggregate = aggregateStore.Get<T>(id);
            //AddTrackedAggregate(aggregate, aggregateStore);
            return aggregate;
        }

        public async Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            var aggregateStore = GetAggregateStore<T>();
            T aggregate = await aggregateStore.GetAsync<T>(id);
            //AddTrackedAggregate(aggregate, aggregateStore);
            return aggregate;
        }

        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            return aggregateStore.Where(predicate);
        }

        public void Remove<T>(T aggregate) where T : class, IAggregateRoot
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            aggregateStore.Remove(aggregate);
            //RemoveTrackedAggregate(aggregate, aggregateStore);
        }

        /*private void AddTrackedAggregate(IAggregateRoot aggregate, IAggregateStore aggregateStore)
        {
            aggregates.Add(aggregate.Id, aggregate);
            aggregateStores[aggregateStore].Add(aggregate);
        }

        private void RemoveTrackedAggregate(IAggregateRoot aggregate, IAggregateStore aggregateStore)
        {
            if (aggregateStore != null)
            {
                aggregates.Remove(aggregate.Id);
                aggregateStores[aggregateStore].Remove(aggregate);
            }
        }*/

        protected class RepositoryTransaction : ITransaction
        {
            private readonly Repository repository;

            public RepositoryTransaction(Repository repository)
            {
                this.repository = repository;
            }

            public void Commit()
            {
                repository.SaveChanges();
            }

            public async Task CommitAsync()
            {
                await repository.SaveChangesAsync();
            }

            public void Dispose()
            {
            }
        }
    }
}
