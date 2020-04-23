using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions;
using Revo.DataAccess.Entities;

namespace Revo.RavenDB.DataAccess
{
    public class RavenCrudRepository : IRavenCrudRepository
    {
        private readonly IAsyncDocumentSession asyncDocumentSession;
        private readonly HashSet<object> addedEntities = new HashSet<object>();

        public RavenCrudRepository(IAsyncDocumentSession asyncDocumentSession)
        {
            this.asyncDocumentSession = asyncDocumentSession;
        }

        public void Dispose()
        {
        }

        public IEnumerable<IRepositoryFilter> DefaultFilters { get; }

        public T Get<T>(object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public T Get<T>(object id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(params object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(CancellationToken cancellationToken, params object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetAsync<T>(object id) where T : class
        {
            T t = await asyncDocumentSession.LoadAsync<T>(GetRavenId<T>(id.ToString()));
            Revo.DataAccess.Entities.RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(CancellationToken cancellationToken, object id) where T : class
        {
            T t = await asyncDocumentSession.LoadAsync<T>(GetRavenId<T>(id.ToString()), cancellationToken);
            Revo.DataAccess.Entities.RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
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
            throw new NotImplementedException();
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class
        {
            return asyncDocumentSession.Query<T>().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) where T : class
        {
            return asyncDocumentSession.Query<T>().FirstAsync(predicate, cancellationToken);
        }

        public T Find<T>(object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public T Find<T>(object id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> FindAsync<T>(params object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> FindAsync<T>(CancellationToken cancellationToken, params object[] id) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<T> FindAsync<T>(object id) where T : class
        {
            return asyncDocumentSession.LoadAsync<T>(GetRavenId<T>(id.ToString()));
        }

        public Task<T> FindAsync<T>(CancellationToken cancellationToken, object id) where T : class
        {
            return asyncDocumentSession.LoadAsync<T>(GetRavenId<T>(id.ToString()), cancellationToken);
        }

        public IQueryable<T> FindAll<T>() where T : class
        {
            return asyncDocumentSession.Query<T>();
        }

        public async Task<T[]> FindAllAsync<T>(CancellationToken cancellationToken) where T : class
        {
            var result = await asyncDocumentSession.Query<T>().ToListAsync(cancellationToken);
            return result.ToArray();
        }

        public IEnumerable<T> FindAllWithAdded<T>() where T : class
        {
            throw new NotImplementedException();
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
            return asyncDocumentSession.Query<T>().Where(predicate);
        }

        public IEnumerable<T> WhereWithAdded<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetEntities<T>(params EntityState[] entityStates) where T : class
        {
            throw new NotImplementedException();
        }

        public EntityState GetEntityState<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public void SetEntityState<T>(T entity, EntityState state) where T : class
        {
            throw new NotImplementedException();
        }

        public bool IsAttached<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public void Attach<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public void AttachRange<T>(IEnumerable<T> entities) where T : class
        {
            throw new NotImplementedException();
        }

        public void Add<T>(T entity) where T : class
        {
            addedEntities.Add(entity);
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            throw new NotImplementedException();
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
            var objectProperty = Expression.Lambda<Func<T, object>>(
                Expression.Convert(navigationPropertyPath.Body, typeof(object)),
                navigationPropertyPath.Parameters[0]);
            return queryable.Include(objectProperty);
        }

        public IQueryable<T> Include<T>(IQueryable<T> queryable, string navigationPropertyPath) where T : class
        {
            return queryable.Include(navigationPropertyPath);
        }

        public async Task<long> LongCountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await queryable.CountAsync(cancellationToken);
        }

        public Task<T> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.FirstOrDefaultAsync(cancellationToken);
        }

        public Task<T> FirstAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.FirstAsync(cancellationToken);
        }

        public async Task<T[]> ToArrayAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return (await queryable.ToListAsync(cancellationToken)).ToArray();
        }

        public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken))
        {
            return queryable.ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(IQueryable<T> queryable, Func<T, TKey> keySelector,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return (await queryable.ToListAsync(cancellationToken)).ToDictionary(keySelector);
        }

        public async Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(IQueryable<T> queryable, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return (await queryable.ToListAsync(cancellationToken)).ToDictionary(keySelector, comparer);
        }

        public async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(IQueryable<T> queryable, Func<T, TKey> keySelector, Func<T, TElement> elementSelector,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return (await queryable.ToListAsync(cancellationToken)).ToDictionary(keySelector, elementSelector);
        }

        public async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(IQueryable<T> queryable, Func<T, TKey> keySelector, Func<T, TElement> elementSelector,
            IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return (await queryable.ToListAsync(cancellationToken)).ToDictionary(keySelector, elementSelector, comparer);
        }

        public void Remove<T>(T entity) where T : class
        {
            asyncDocumentSession.Delete(GetRavenId(entity));
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            foreach (object entity in addedEntities)
            {
                await asyncDocumentSession.StoreAsync(entity,
                    GetRavenId(entity), cancellationToken);
            }

            try
            {
                await asyncDocumentSession.SaveChangesAsync(cancellationToken);
                addedEntities.Clear();
            }
            catch (ConcurrencyException e)
            {
                throw new OptimisticConcurrencyException($"Optimistic concurrency exception occurred while saving Raven repository", e);
            }
        }

        protected string GetRavenId<T>(T entity)
        {
            if (entity is IHasId<Guid> hasGuid)
            {
                return GetRavenId(entity.GetType(), hasGuid.Id.ToString());
            }
            else
            {
                throw new ArgumentException($"Cannot deduce RavenDB document ID for entity of type: {entity.GetType().FullName}");
            }
        }

        protected string GetRavenId<T>(string id)
        {
            return GetRavenId(typeof(T), id);
        }

        protected string GetRavenId(Type entityType, string id)
        {
            return entityType.Name + "/" + id;
        }
    }
}
