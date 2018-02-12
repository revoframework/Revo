using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Domain;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Revo.Domain.Core;

namespace Revo.DataAccess.RavenDB.Entities
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
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
        }

        public async Task<T> GetAsync<T>(CancellationToken cancellationToken, object id) where T : class
        {
            T t = await asyncDocumentSession.LoadAsync<T>(GetRavenId<T>(id.ToString()), cancellationToken);
            RepositoryHelpers.ThrowIfGetFailed<T>(t, id);
            return t;
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

        public async Task<IList<T>> FindAllAsync<T>(CancellationToken cancellationToken) where T : class
        {
            return await asyncDocumentSession.Query<T>().ToListAsync(cancellationToken);
        }

        public IEnumerable<T> FindAllWithAdded<T>() where T : class
        {
            throw new NotImplementedException();
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

            await asyncDocumentSession.SaveChangesAsync(cancellationToken);
            addedEntities.Clear();
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
