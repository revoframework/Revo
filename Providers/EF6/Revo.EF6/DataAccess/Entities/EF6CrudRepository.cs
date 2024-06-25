using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using EntityState = System.Data.Entity.EntityState;

namespace Revo.EF6.DataAccess.Entities
{
    public class EF6CrudRepository : IEF6CrudRepository, IFilteringRepository<IEF6CrudRepository>, ITransactionProvider
    {
        private readonly IRepositoryFilter[] repositoryFilters;

        public EF6CrudRepository(IEF6DatabaseAccess databaseAccess, IRepositoryFilter[] repositoryFilters)
        {
            this.repositoryFilters = repositoryFilters;
            DatabaseAccess = databaseAccess;
        }

        public IEF6DatabaseAccess DatabaseAccess { get; }
        public IEnumerable<IRepositoryFilter> DefaultFilters => repositoryFilters;

        public void Attach<T>(T entity) where T : class
        {
            DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Attach(entity);
        }

        public void AttachRange<T>(IEnumerable<T> entities) where T : class
        {
            foreach (T entity in entities)
            {
                DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Attach(entity);
            }
        }

        public void Add<T>(T entity) where T : class
        {
            DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Add(entity);
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            DatabaseAccess.GetDbContext(typeof(T)).Set<T>().AddRange(entities);
        }

        public T Get<T>(object id) where T : class
        {
            T t = DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Find(id);
            t = FilterResult(t);
            Revo.DataAccess.Entities.RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public T Get<T>(params object[] id) where T : class
        {
            T t = DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Find(id);
            t = FilterResult(t);
            Revo.DataAccess.Entities.RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(object[] id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(id);
            t = FilterResult(t);
            Revo.DataAccess.Entities.RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(CancellationToken cancellationToken, object[] id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(cancellationToken, id);
            t = FilterResult(t);
            Revo.DataAccess.Entities.RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(object id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(id);
            t = FilterResult(t);
            Revo.DataAccess.Entities.RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(CancellationToken cancellationToken, object id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(cancellationToken, id);
            t = FilterResult(t);
            Revo.DataAccess.Entities.RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public Task<T[]> GetManyAsync<T, TId>(params TId[] ids) where T : class, IHasId<TId>
        {
            return GetManyAsync<T, TId>(default(CancellationToken), ids);
        }

        public async Task<T[]> GetManyAsync<T, TId>(CancellationToken cancellationToken, params TId[] ids) where T : class, IHasId<TId>
        {
            var result = await FindManyAsync<T, TId>(cancellationToken, ids);
            RepositoryHelpers.ThrowIfGetManyFailed(result, ids);
            return result;
        }

        public async Task<T[]> GetManyAsync<T>(params Guid[] ids) where T : class, IHasId<Guid>
        {
            var result = await FindManyAsync<T, Guid>(default, ids);
            RepositoryHelpers.ThrowIfGetManyFailed(result, ids);
            return result;
        }

        public async Task<T[]> GetManyAsync<T>(CancellationToken cancellationToken, params Guid[] ids) where T : class, IHasId<Guid>
        {
            var result = await FindManyAsync<T, Guid>(cancellationToken, ids);
            RepositoryHelpers.ThrowIfGetManyFailed(result, ids);
            return result;
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return FilterResult(DatabaseAccess.GetDbContext(typeof(T)).Set<T>())
                .FirstOrDefault(predicate);
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return FilterResults(DatabaseAccess.GetDbContext(typeof(T)).Set<T>())
                .First(predicate);
        }

        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class
        {
            return await FilterResults(DatabaseAccess.GetDbContext(typeof(T)).Set<T>())
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class
        {
            return await FilterResults(DatabaseAccess.GetDbContext(typeof(T)).Set<T>())
                .FirstAsync(predicate, cancellationToken);
        }

        public T Find<T>(object id) where T : class
        {
            T t = DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Find(id);
            t = FilterResult(t);
            return t;
        }

        public T Find<T>(params object[] id) where T : class
        {
            T t = DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Find(id);
            t = FilterResult(t);
            return t;
        }

        public async Task<T> FindAsync<T>(object[] id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(id);
            t = FilterResult(t);
            return t;
        }

        public async Task<T> FindAsync<T>(CancellationToken cancellationToken, object[] id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(cancellationToken, id);
            t = FilterResult(t);
            return t;
        }

        public async Task<T> FindAsync<T>(object id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(id);
            t = FilterResult(t);
            return t;
        }

        public async Task<T> FindAsync<T>(CancellationToken cancellationToken, object id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(cancellationToken, id);
            t = FilterResult(t);
            return t;
        }

        public IQueryable<T> FindAll<T>() where T : class
        {
            return FilterResults(DatabaseAccess.GetDbContext(typeof(T)).Set<T>());
        }
        
        public async Task<T[]> FindAllAsync<T>(CancellationToken cancellationToken) where T : class
        {
            return await FilterResults(DatabaseAccess.GetDbContext(typeof(T)).Set<T>()).ToArrayAsync(cancellationToken);
        }

        public IEnumerable<T> FindAllWithAdded<T>() where T : class
        {
            var addedEntities = DatabaseAccess.GetDbContext(typeof(T)).ChangeTracker.Entries<T>()
                .Where(x => x.State == System.Data.Entity.EntityState.Added)
                .Select(x => x.Entity);

            return FilterResults(DatabaseAccess.GetDbContext(typeof(T)).Set<T>())
                .Union(addedEntities);
        }
        
        public Task<T[]> FindManyAsync<T, TId>(params TId[] ids) where T : class, IHasId<TId>
        {
            return FindManyAsync<T, TId>(default(CancellationToken), ids);
        }

        public async Task<T[]> FindManyAsync<T, TId>(CancellationToken cancellationToken, params TId[] ids) where T : class, IHasId<TId>
        {
            var dbContext = DatabaseAccess.GetDbContext(typeof(T));
            var loaded = dbContext.ChangeTracker.Entries<T>().Where(x => ids.Contains(x.Entity.Id))
                .Select(x => x.Entity).ToArray();
            var missingIds = ids.Where(id => !loaded.Any(x => Equals(x.Id, id))).ToArray();

            var result = await Where<T>(x => missingIds.Contains(x.Id)).ToArrayAsync(cancellationToken);
            return loaded.Concat(result).ToArray();
        }

        public Task<T[]> FindManyAsync<T>(params Guid[] ids) where T : class, IHasId<Guid>
        {
            return FindManyAsync<T, Guid>(default(CancellationToken), ids);
        }

        public Task<T[]> FindManyAsync<T>(CancellationToken cancellationToken, params Guid[] ids) where T : class, IHasId<Guid>
        {
            return FindManyAsync<T, Guid>(cancellationToken, ids);
        }

        public Task<bool> AnyAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.AnyAsync(cancellationToken);
        }

        public Task<int> CountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.CountAsync(cancellationToken);
        }

        public IQueryable<T> Include<T, TProperty>(IQueryable<T> queryable, Expression<Func<T, TProperty>> navigationPropertyPath) where T : class
        {
            return queryable.Include(navigationPropertyPath);
        }

        public IQueryable<T> Include<T>(IQueryable<T> queryable, string navigationPropertyPath) where T : class
        {
            return queryable.Include(navigationPropertyPath);
        }

        public Task<long> LongCountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.LongCountAsync(cancellationToken);
        }

        public Task<T> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.FirstOrDefaultAsync(cancellationToken);
        }

        public Task<T> FirstAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.FirstAsync(cancellationToken);
        }

        public Task<T[]> ToArrayAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.ToArrayAsync(cancellationToken);
        }

        public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.ToListAsync(cancellationToken);
        }

        public Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(IQueryable<T> queryable, Func<T, TKey> keySelector,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.ToDictionaryAsync(keySelector, cancellationToken);
        }

        public Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(IQueryable<T> queryable, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.ToDictionaryAsync(keySelector, comparer, cancellationToken);
        }

        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(IQueryable<T> queryable, Func<T, TKey> keySelector, Func<T, TElement> elementSelector,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.ToDictionaryAsync(keySelector, elementSelector, cancellationToken);
        }

        public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(IQueryable<T> queryable, Func<T, TKey> keySelector, Func<T, TElement> elementSelector,
            IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.ToDictionaryAsync(keySelector, elementSelector, comparer, cancellationToken);
        }

        public void Remove<T>(T entity) where T : class
        {
            if (IsAttached(entity))
            {
                DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Remove(entity);
            }
        }

        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return FilterResults(DatabaseAccess.GetDbContext(typeof(T)).Set<T>())
                .Where(predicate);
        }

        public IEnumerable<T> WhereWithAdded<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            IEnumerable<T> addedEntities = DatabaseAccess.GetDbContext(typeof(T)).ChangeTracker.Entries<T>()
                .Where(x => x.State == System.Data.Entity.EntityState.Added)
                .Select(x => x.Entity)
                .Where(predicate.Compile());

            return FilterResults(DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Where(predicate)).ToList()
                .Union(addedEntities);
        }

        public IEnumerable<T> GetEntities<T>(params Revo.DataAccess.Entities.EntityState[] entityStates) where T : class
        {
            var efStates = entityStates.Select(x => EntityStateToEF(x)).ToArray();

            var entries = DatabaseAccess.DbContexts.Values.SelectMany(x => x.ChangeTracker.Entries());
            if (entityStates?.Length > 0)
            {
                entries = entries.Where(x => efStates.Any(s => (s & x.State) == s));
            }

            return entries.Select(x => x.Entity)
                .OfType<T>();
        }

        public Revo.DataAccess.Entities.EntityState GetEntityState<T>(T entity) where T : class
        {
            return EntityStateFromEF(DatabaseAccess.GetDbContext(typeof(T)).Entry(entity).State);
        }

        public void SetEntityState<T>(T entity, Revo.DataAccess.Entities.EntityState state) where T : class
        {
            DatabaseAccess.GetDbContext(typeof(T)).Entry(entity).State = EntityStateToEF(state);
        }

        public bool IsAttached<T>(T entity) where T : class
        {
            return DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Local.Any(x => x == entity);
        }

        public IEF6CrudRepository IncludeFilters(params IRepositoryFilter[] repositoryFilters)
        {
            return new EF6CrudRepository(DatabaseAccess,
                this.repositoryFilters.Union(repositoryFilters).ToArray());
        }

        public IEF6CrudRepository ExcludeFilter(params IRepositoryFilter[] repositoryFilters)
        {
            return new EF6CrudRepository(DatabaseAccess,
                this.repositoryFilters.Except(repositoryFilters).ToArray());
        }

        public IEF6CrudRepository ExcludeFilters<TRepositoryFilter>() where TRepositoryFilter : IRepositoryFilter
        {
            return new EF6CrudRepository(DatabaseAccess,
                this.repositoryFilters.Where(x => !typeof(TRepositoryFilter).IsAssignableFrom(x.GetType())).ToArray());
        }

        public void SaveChanges()
        {
            List<DbContext> modifiedDbContexts = DatabaseAccess.DbContexts.Values.Where(x => x.ChangeTracker.HasChanges()).ToList();
            if (modifiedDbContexts.Count == 0)
            {
                return;
            }
            else if (modifiedDbContexts.Count > 1)
            {
                throw new InvalidOperationException(
                    "It is not allowed to modify entities from two or more different schema spaces at a time");
            }

            DbContext dbContext = modifiedDbContexts.First();
            IncrementEntityVersions(dbContext);
            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                IncrementEntityVersions(dbContext, -1);

                if (e is DbUpdateConcurrencyException ce)
                {
                    throw new OptimisticConcurrencyException(
                        $"Optimistic concurrency exception occurred while saving EF Core repository entities: {string.Join(", ", ce.Entries.Select(x => $"{x.Entity} ({x.State})"))}",
                        e);
                }

                throw;
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            List<DbContext> modifiedDbContexts = DatabaseAccess.DbContexts.Values.Where(x => x.ChangeTracker.HasChanges()).ToList();
            if (modifiedDbContexts.Count == 0)
            {
                return;
            }
            else if (modifiedDbContexts.Count > 1)
            {
                throw new InvalidOperationException(
                    "It is not allowed to modify entities from two or more different schema spaces at a time");
            }

            DbContext dbContext = modifiedDbContexts.First();
            FilterSavedEntities(dbContext);
            IncrementEntityVersions(dbContext);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                IncrementEntityVersions(dbContext, -1);

                if (e is DbUpdateConcurrencyException ce)
                {
                    throw new OptimisticConcurrencyException(
                        $"Optimistic concurrency exception occurred while saving EF Core repository entities: {string.Join(", ", ce.Entries.Select(x => $"{x.Entity} ({x.State})"))}",
                        e);
                }

                throw;
            }
        }

        private void FilterSavedEntities(DbContext dbContext)
        {
            foreach (var entry in dbContext.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        FilterAdded(entry.Entity); // TODO should cast
                        break;
                    case EntityState.Deleted:
                        FilterDeleted(entry.Entity); // TODO should cast
                        break;
                    case EntityState.Modified:
                        FilterModified(entry.Entity); // TODO should cast
                        break;
                    case EntityState.Detached:
                    case EntityState.Unchanged:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected Revo.DataAccess.Entities.EntityState EntityStateFromEF(System.Data.Entity.EntityState entityState)
        {
            return (Revo.DataAccess.Entities.EntityState)entityState;
        }

        protected System.Data.Entity.EntityState EntityStateToEF(Revo.DataAccess.Entities.EntityState entityState)
        {
            return (System.Data.Entity.EntityState)entityState;
        }

        private void IncrementEntityVersions(DbContext dbContext, int increment = 1)
        {
            var changedEntities = dbContext
                .ChangeTracker.Entries()
                .Where(x => x.State == System.Data.Entity.EntityState.Modified)
                .Select(x => x.Entity)
                .OfType<IRowVersioned>();

            foreach (var entity in changedEntities)
            {
                entity.Version += increment;
            }
        }

        public IEnumerable<IDbEntityEntry> Entries()
        {
            return DatabaseAccess.DbContexts.Values.SelectMany(x => x.ChangeTracker.Entries())
                .Select(x => new WrapperDbEntityEntry(x));
        }

        public IDbEntityEntry Entry(object entity)
        {
            return new WrapperDbEntityEntry(DatabaseAccess.GetDbContext(entity.GetType()).Entry(entity));
        }

        public IDbEntityEntry<T> Entry<T>(T entity) where T : class
        {
            return new WrapperDbEntityEntry<T>(DatabaseAccess.GetDbContext(typeof(T)).Entry<T>(entity));
        }

        public ITransaction CreateTransaction()
        {
            return new RepositoryTransaction(this);
        }

        public void Dispose()
        {
        }
        
        private IQueryable<T> FilterResults<T>(IQueryable<T> results) where T : class
        {
            var intermed = results;
            foreach (var repositoryFilter in repositoryFilters)
            {
                intermed = repositoryFilter.FilterResults(intermed);
            }

            return intermed;
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
    }
}
