using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Revo.DataAccess.Entities;
using EntityState = Revo.DataAccess.Entities.EntityState;

namespace Revo.EFCore.DataAccess.Entities
{
    public class EFCoreCrudRepository :
        IEFCoreCrudRepository,
        IFilteringRepository<IEFCoreCrudRepository>
    {
        private readonly IRepositoryFilter[] repositoryFilters;

        public EFCoreCrudRepository(IRepositoryFilter[] repositoryFilters,
            IEFCoreDatabaseAccess databaseAccess)
        {
            this.repositoryFilters = repositoryFilters;
            DatabaseAccess = databaseAccess;
        }

        public IEFCoreDatabaseAccess DatabaseAccess { get; }

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

        public void Dispose()
        {
        }

        public T Get<T>(object id) where T : class
        {
            T t = DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Find(id);
            t = FilterResult(t);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public T Get<T>(params object[] id) where T : class
        {
            T t = DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Find(id);
            t = FilterResult(t);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(object[] id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(id);
            t = FilterResult(t);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(CancellationToken cancellationToken, object[] id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(id, cancellationToken);
            t = FilterResult(t);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(object id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(id);
            t = FilterResult(t);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(CancellationToken cancellationToken, object id) where T : class
        {
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(new[] { id }, cancellationToken);
            t = FilterResult(t);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
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
            return FindAll<T>()
                .FirstOrDefault(predicate);
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return FindAll<T>()
                .First(predicate);
        }

        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class
        {
            return await FindAll<T>()
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class
        {
            return await FindAll<T>()
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
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(id, cancellationToken);
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
            T t = await DatabaseAccess.GetDbContext(typeof(T)).Set<T>().FindAsync(new[] { id }, cancellationToken);
            t = FilterResult(t);
            return t;
        }

        public IQueryable<T> FindAll<T>() where T : class
        {
            var dbContext = DatabaseAccess.GetDbContext(typeof(T));
            return FilterResults(dbContext.Set<T>());
        }

        public async Task<T[]> FindAllAsync<T>(CancellationToken cancellationToken) where T : class
        {
            return await FindAll<T>().ToArrayAsync(cancellationToken);
        }

        public IEnumerable<T> FindAllWithAdded<T>() where T : class
        {
            var addedEntities = DatabaseAccess.GetDbContext(typeof(T)).ChangeTracker.Entries<T>()
                .Where(x => x.State == Microsoft.EntityFrameworkCore.EntityState.Added)
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

        public IQueryable<T> FromSqlInterpolated<T>(FormattableString sql) where T : class
        {
            var results = DatabaseAccess.FromSqlInterpolated<T>(sql);
            return FilterResults(results);
        }

        public IQueryable<T> FromSqlRaw<T>(string sql, params object[] parameters) where T : class
        {
            var results = DatabaseAccess.FromSqlRaw<T>(sql, parameters);
            return FilterResults(results);
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
            return FindAll<T>().Where(predicate);
        }

        public IEnumerable<T> WhereWithAdded<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            IEnumerable<T> addedEntities = DatabaseAccess.GetDbContext(typeof(T)).ChangeTracker.Entries<T>()
                .Where(x => x.State == Microsoft.EntityFrameworkCore.EntityState.Added)
                .Select(x => x.Entity)
                .Where(predicate.Compile());

            return FilterResults(DatabaseAccess.GetDbContext(typeof(T)).Set<T>().Where(predicate)).ToList()
                .Union(addedEntities);
        }

        public IEnumerable<T> GetEntities<T>(params Revo.DataAccess.Entities.EntityState[] entityStates) where T : class
        {
            var efStates = entityStates.Select(x => EntityStateToEF(x)).ToArray();

            var entries = DatabaseAccess.DbContexts.Values.SelectMany(x => x.ChangeTracker.Entries<T>());
            if (entityStates?.Length > 0)
            {
                entries = entries.Where(x => efStates.Contains(x.State));
            }

            return entries.Select(x => x.Entity);
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
            return DatabaseAccess.GetDbContext(typeof(T)).Entry(entity).State !=
                   Microsoft.EntityFrameworkCore.EntityState.Detached;
        }

        public IEFCoreCrudRepository IncludeFilters(params IRepositoryFilter[] repositoryFilters)
        {
            return new EFCoreCrudRepository(
                this.repositoryFilters.Union(repositoryFilters).ToArray(),
                DatabaseAccess);
        }

        public IEFCoreCrudRepository ExcludeFilter(params IRepositoryFilter[] repositoryFilters)
        {
            return new EFCoreCrudRepository(
                this.repositoryFilters.Except(repositoryFilters).ToArray(),
                DatabaseAccess);
        }

        public IEFCoreCrudRepository ExcludeFilters<TRepositoryFilter>() where TRepositoryFilter : IRepositoryFilter
        {
            return new EFCoreCrudRepository(
                this.repositoryFilters.Where(x => !typeof(TRepositoryFilter).IsAssignableFrom(x.GetType())).ToArray(),
                DatabaseAccess);
        }

        public void SaveChanges()
        {
            List<DbContext> modifiedDbContexts = DatabaseAccess.DbContexts.Values
                .Where(x => x.ChangeTracker.HasChanges())
                .ToList();

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
            List<DbContext> modifiedDbContexts = DatabaseAccess.DbContexts.Values
                .Where(x => x.ChangeTracker.HasChanges())
                .ToList();

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
                    case Microsoft.EntityFrameworkCore.EntityState.Added:
                        FilterAdded(entry.Entity); // TODO should cast
                        break;
                    case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                        FilterDeleted(entry.Entity); // TODO should cast
                        break;
                    case Microsoft.EntityFrameworkCore.EntityState.Modified:
                        FilterModified(entry.Entity); // TODO should cast
                        break;
                    case Microsoft.EntityFrameworkCore.EntityState.Detached:
                    case Microsoft.EntityFrameworkCore.EntityState.Unchanged:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected Revo.DataAccess.Entities.EntityState EntityStateFromEF(Microsoft.EntityFrameworkCore.EntityState entityState)
        {
            switch (entityState)
            {
                case Microsoft.EntityFrameworkCore.EntityState.Detached:
                    return EntityState.Detached;
                case Microsoft.EntityFrameworkCore.EntityState.Unchanged:
                    return EntityState.Unchanged;
                case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                    return EntityState.Deleted;
                case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    return EntityState.Modified;
                case Microsoft.EntityFrameworkCore.EntityState.Added:
                    return EntityState.Added;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entityState), entityState, null);
            }
        }

        protected Microsoft.EntityFrameworkCore.EntityState EntityStateToEF(Revo.DataAccess.Entities.EntityState entityState)
        {
            switch (entityState)
            {
                case EntityState.Detached:
                    return Microsoft.EntityFrameworkCore.EntityState.Detached;
                case EntityState.Unchanged:
                    return Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                case EntityState.Added:
                    return Microsoft.EntityFrameworkCore.EntityState.Added;
                case EntityState.Deleted:
                    return Microsoft.EntityFrameworkCore.EntityState.Deleted;
                case EntityState.Modified:
                    return Microsoft.EntityFrameworkCore.EntityState.Modified;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entityState), entityState, null);
            }
        }

        private void IncrementEntityVersions(DbContext dbContext, int increment = 1)
        {
            var changedEntities = dbContext
                .ChangeTracker.Entries()
                .Where(x => x.State == Microsoft.EntityFrameworkCore.EntityState.Modified)
                .Select(x => x.Entity)
                .OfType<IRowVersioned>();

            foreach (var entity in changedEntities)
            {
                entity.Version += increment;
            }
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
