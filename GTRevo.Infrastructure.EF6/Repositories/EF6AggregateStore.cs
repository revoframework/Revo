using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.DataAccess.EF6.Model;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Repositories;

namespace GTRevo.Infrastructure.EF6.Repositories
{
    public class EF6AggregateStore : IQueryableAggregateStore
    {
        private readonly ICrudRepository crudRepository;
        private readonly IModelMetadataExplorer modelMetadataExplorer;

        public EF6AggregateStore(ICrudRepository crudRepository,
            IModelMetadataExplorer modelMetadataExplorer)
        {
            this.crudRepository = crudRepository;
            this.modelMetadataExplorer = modelMetadataExplorer;
        }

        public void Add<T>(T aggregate) where T : class, IAggregateRoot
        {
            crudRepository.Add(aggregate);
        }

        public T Get<T>(Guid id) where T : class, IAggregateRoot
        {
            return crudRepository.Get<T>(id);
        }

        public Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return crudRepository.GetAsync<T>(id);
        }

        public IEnumerable<IAggregateRoot> GetTrackedAggregates()
        {
            return crudRepository.Entries()
                .Select(x => x.Entity)
                .OfType<IAggregateRoot>();
        }

        public bool CanHandleAggregateType(Type aggregateType)
        {
            return modelMetadataExplorer.IsTypeMapped(aggregateType);
        }

        public void SaveChanges()
        {
            crudRepository.SaveChanges();
        }

        public Task SaveChangesAsync()
        {
            return crudRepository.SaveChangesAsync();
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FirstOrDefault(predicate);
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.First(predicate);
        }

        public Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FirstOrDefaultAsync(predicate);
        }

        public Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FirstAsync(predicate);
        }

        public IQueryable<T> FindAll<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FindAll<T>();
        }

        public Task<IList<T>> FindAllAsync<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FindAllAsync<T>();
        }

        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.Where(predicate);
        }

        public void Remove<T>(T aggregate) where T : class, IAggregateRoot
        {
            crudRepository.Remove(aggregate);
        }
    }
}
