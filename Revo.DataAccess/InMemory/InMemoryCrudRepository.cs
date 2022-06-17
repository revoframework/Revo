using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;

namespace Revo.DataAccess.InMemory
{
    public class InMemoryCrudRepository : ICrudRepository
    {
        public InMemoryCrudRepository()
        {
        }

        public IEnumerable<IRepositoryFilter> DefaultFilters => Filters;
        
        protected List<EntityEntry> EntityEntries { get; } = new List<EntityEntry>();
        protected List<IRepositoryFilter> Filters { get; } = new List<IRepositoryFilter>();

        public T Get<T>(object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public T Get<T>(object id) where T : class
        {
            return EntityEntries.Select(x => x.Instance).OfType<T>().First(x => HasEntityId(x, id));
        }

        public Task<T> GetAsync<T>(CancellationToken cancellationToken, params object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(params object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetAsync<T>(object id) where T : class
        {
            var result = EntityEntries.Select(x => x.Instance)
                .OfType<T>().FirstOrDefault(x => HasEntityId(x, id));
            RepositoryHelpers.ThrowIfGetFailed(result, id);
            return result;
        }

        public Task<T> GetAsync<T>(CancellationToken cancellationToken, object id) where T : class
        {
            return GetAsync<T>(id);
        }

        public Task<T[]> GetManyAsync<T, TId>(params TId[] ids) where T : class, IHasId<TId>
        {
            return GetManyAsync<T, TId>(default(CancellationToken), ids);
        }

        public async Task<T[]> GetManyAsync<T, TId>(CancellationToken cancellationToken, params TId[] ids) where T : class, IHasId<TId>
        {
            var result = await FindManyAsync<T, TId>(default(CancellationToken), ids);
            RepositoryHelpers.ThrowIfGetManyFailed(result, ids);
            return result;
        }

        public Task<T[]> GetManyAsync<T>(params Guid[] ids) where T : class, IHasId<Guid>
        {
            return GetManyAsync<T, Guid>(default(CancellationToken), ids);
        }

        public Task<T[]> GetManyAsync<T>(CancellationToken cancellationToken, params Guid[] ids) where T : class, IHasId<Guid>
        {
            return GetManyAsync<T, Guid>(cancellationToken, ids);
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return FindAll<T>().FirstOrDefault(predicate.Compile());
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return FindAll<T>().First(predicate.Compile());
        }

        public Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.FromResult(FindAll<T>().FirstOrDefault(predicate.Compile()));
        }

        public Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.FromResult(First(predicate));
        }

        public T Find<T>(object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public T Find<T>(object id) where T : class
        {
            return EntityEntries
                .Select(x => x.Instance)
                .OfType<T>()
                .FirstOrDefault(x => HasEntityId(x, id));
        }

        public Task<T> FindAsync<T>(CancellationToken cancellationToken, params object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> FindAsync<T>(params object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> FindAsync<T>(CancellationToken cancellationToken, object id) where T : class
        {
            return FindAsync<T>(id);
        }

        public Task<T> FindAsync<T>(object id) where T : class
        {
            return Task.FromResult(Find<T>(id));
        }

        public IQueryable<T> FindAll<T>() where T : class
        {
            return CreateQueryable(EntityEntries
                .Where(x => (x.State & EntityState.Added) == 0 && (x.State & EntityState.Detached) == 0)
                .Select(x => x.Instance)
                .OfType<T>());
        }

        public Task<T[]> FindAllAsync<T>(CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.FromResult(FindAll<T>().ToArray());
        }

        public IEnumerable<T> FindAllWithAdded<T>() where T : class
        {
            return EntityEntries
                   .Select(x => x.Instance)
                   .OfType<T>();
        }

        public Task<T[]> FindManyAsync<T, TId>(params TId[] ids) where T : class, IHasId<TId>
        {
            return FindManyAsync<T, TId>(default(CancellationToken), ids);
        }

        public Task<T[]> FindManyAsync<T, TId>(CancellationToken cancellationToken, params TId[] ids) where T : class, IHasId<TId>
        {
            return Task.FromResult(Where<T>(x => ids.Contains(x.Id)).ToArray());
        }

        public Task<T[]> FindManyAsync<T>(params Guid[] ids) where T : class, IHasId<Guid>
        {
            return FindManyAsync<T, Guid>(default(CancellationToken), ids);
        }

        public Task<T[]> FindManyAsync<T>(CancellationToken cancellationToken, params Guid[] ids) where T : class, IHasId<Guid>
        {
            return FindManyAsync<T, Guid>(cancellationToken, ids);
        }

        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return FindAll<T>().Where(predicate);
        }

        public IEnumerable<T> WhereWithAdded<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return FindAllWithAdded<T>().Where(predicate.Compile());
        }

        public IEnumerable<T> GetEntities<T>(params EntityState[] entityStates) where T : class
        {
            IEnumerable<EntityEntry> entries = EntityEntries;
            if (entityStates?.Length > 0)
            {
                entries = entries.Where(x => entityStates.Any(s => (s & x.State) == s));
            }

            return entries
                .Select(x => x.Instance)
                .OfType<T>();
        }

        public EntityState GetEntityState<T>(T entity) where T : class
        {
            return EntityEntries.FirstOrDefault(x => x.Instance == entity)
                       ?.State ?? EntityState.Detached;
        }

        public void SetEntityState<T>(T entity, EntityState state) where T : class
        {
            EntityEntry entry = EntityEntries.FirstOrDefault(x => x.Instance == entity);
            if (entry == null)
            {
                EntityEntries.Add(new EntityEntry(entity, state));
            }
            else
            {
                entry.State = state;
            }
        }

        public bool IsAttached<T>(T entity) where T : class
        {
            return EntityEntries.Any(x => x.Instance == entity && (x.State & EntityState.Detached) != 0);
        }

        public IReadRepository IncludeFilters(params IRepositoryFilter[] repositoryFilters)
        {
            throw new NotImplementedException();
        }

        public IReadRepository ExcludeFilter(params IRepositoryFilter[] repositoryFilters)
        {
            throw new NotImplementedException();
        }

        public IReadRepository ExcludeFilters<TRepositoryFilter>() where TRepositoryFilter : IRepositoryFilter
        {
            throw new NotImplementedException();
        }

        public void Attach<T>(T entity) where T : class
        {
            if (!IsAttached(entity))
            {
                EntityEntries.Add(new EntityEntry(entity, EntityState.Unchanged));
            }
        }

        public void AttachRange<T>(IEnumerable<T> entities) where T : class
        {
            foreach (T entity in entities)
            {
                Attach(entity);
            }
        }

        public void Add<T>(T entity) where T : class
        {
            if (!IsAttached(entity))
            {
                EntityEntries.Add(new EntityEntry(entity, EntityState.Added));
            }
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            foreach (T entity in entities)
            {
                Add(entity);
            }
        }

        public Task<bool> AnyAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.Any());
        }

        public Task<int> CountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.Count());
        }

        public IQueryable<T> Include<T, TProperty>(IQueryable<T> queryable, Expression<Func<T, TProperty>> navigationPropertyPath) where T : class
        {
            return queryable;
        }

        public IQueryable<T> Include<T>(IQueryable<T> queryable, string navigationPropertyPath) where T : class
        {
            return queryable;
        }

        public Task<long> LongCountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.LongCount());
        }

        public Task<T> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.FirstOrDefault());
        }

        public Task<T> FirstAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.First());
        }

        public Task<T[]> ToArrayAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.ToArray());
        }

        public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.ToList());
        }

        public Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(IQueryable<T> queryable, Func<T, TKey> keySelector,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.ToDictionary(keySelector));
        }

        public Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(IQueryable<T> queryable, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.ToDictionary(keySelector, comparer));
        }

        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(IQueryable<T> queryable, Func<T, TKey> keySelector, Func<T, TElement> elementSelector,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.ToDictionary(keySelector, elementSelector));
        }

        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(IQueryable<T> queryable, Func<T, TKey> keySelector, Func<T, TElement> elementSelector,
            IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(queryable.ToDictionary(keySelector, elementSelector, comparer));
        }

        public virtual void Remove<T>(T entity) where T : class
        {
            EntityEntry entry = EntityEntries.FirstOrDefault(x => x.Instance == entity);
            if (entry != null)
            {
                entry.State = EntityState.Deleted;
            }
        }

        public virtual void SaveChanges()
        {
            foreach (EntityEntry entry in EntityEntries.ToList())
            {
                if ((entry.State & EntityState.Added) != 0 || (entry.State & EntityState.Modified) != 0)
                {
                    entry.State = EntityState.Unchanged;
                }
                else if ((entry.State & EntityState.Deleted) != 0)
                {
                    EntityEntries.Remove(entry);
                }
            }
        }

        public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            SaveChanges();
            return Task.FromResult(0);
        }

        public void Dispose()
        {
        }

        protected virtual IQueryable<T> CreateQueryable<T>(IEnumerable<T> enumerable)
        {
            return enumerable.AsQueryable();
        }

        private object GetEntityId(object entity)
        {
            return entity.GetType().GetProperty("Id").GetValue(entity);
        }

        private bool HasEntityId(object entity, object id)
        {
            object entityId = GetEntityId(entity);
            return entityId.Equals(id);
        }

        public class EntityEntry
        {
            public EntityEntry(object instance, EntityState state)
            {
                Instance = instance;
                State = state;
            }

            public object Instance { get; }
            public EntityState State { get; set; }
        }
    }
}
