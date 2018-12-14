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
        private readonly IAggregateStoreFactory[] aggregateStoreFactories;
        private readonly IUnitOfWork unitOfWork;
        private readonly List<IAggregateStore> aggregateStores = new List<IAggregateStore>();

        public Repository(IAggregateStoreFactory[] aggregateStoreFactories, IUnitOfWork unitOfWork)
        {
            this.aggregateStoreFactories = aggregateStoreFactories;
            this.unitOfWork = unitOfWork;
            unitOfWork.AddInnerTransaction(new RepositoryTransaction(this));
        }

        public IAggregateStore GetAggregateStore(Type entityType)
        {
            var aggregateStore = aggregateStores.FirstOrDefault(x => x.CanHandleAggregateType(entityType));

            if (aggregateStore == null)
            {
                aggregateStore = aggregateStoreFactories.FirstOrDefault(x => x.CanHandleAggregateType(entityType))
                                     ?.CreateAggregateStore(unitOfWork)
                                 ?? throw new InvalidOperationException(
                                     $"No aggregate store for entity type {entityType} was found");
                aggregateStores.Add(aggregateStore);
            }

            return aggregateStore;
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
            var aggregateStore = GetAggregateStore(aggregate.GetType());
            aggregateStore.Add(aggregate);
        }
        
        public void Dispose()
        {
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            T aggregate = aggregateStore.FirstOrDefault(predicate);
            return aggregate;
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            T aggregate = aggregateStore.First(predicate);
            return aggregate;
        }

        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            T aggregate = await aggregateStore.FirstOrDefaultAsync(predicate);
            return aggregate;
        }

        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            var aggregateStore = GetQueyrableAggregateStore<T>();
            T aggregate = await aggregateStore.FirstAsync(predicate);
            return aggregate;
        }

        public T Find<T>(Guid id) where T : class, IAggregateRoot
        {
            var aggregateStore = GetAggregateStore<T>();
            T aggregate = aggregateStore.Find<T>(id);
            return aggregate;
        }

        public async Task<T> FindAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            var aggregateStore = GetAggregateStore<T>();
            T aggregate = await aggregateStore.FindAsync<T>(id);
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
            return aggregate;
        }

        public async Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            var aggregateStore = GetAggregateStore<T>();
            T aggregate = await aggregateStore.GetAsync<T>(id);
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
        }
        
        public async Task SaveChangesAsync()
        {
            var modifiedStores = aggregateStores.Where(x => x.NeedsSave).ToArray();

            if (modifiedStores.Length > 1)
            {
                throw new InvalidOperationException($"It is forbidden to modify aggregates from more than one aggregate store ("
                                                    + $"{string.Join(", ", modifiedStores.Select(x => x.GetType().Name))}) within a single save operation.");
            }

            if (modifiedStores.Length > 0)
            {
                await modifiedStores.First().SaveChangesAsync();
            }
        }

        protected class RepositoryTransaction : ITransaction
        {
            private readonly Repository repository;

            public RepositoryTransaction(Repository repository)
            {
                this.repository = repository;
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
