using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.DataAccess.Entities
{
    public interface IReadRepository : IAsyncQueryableResolver, IDisposable
    {
        IEnumerable<IRepositoryFilter> DefaultFilters { get; }

        T Get<T>(object[] id) where T : class;
        T Get<T>(object id) where T : class;
        Task<T> GetAsync<T>(params object[] id) where T : class;
        Task<T> GetAsync<T>(CancellationToken cancellationToken, params object[] id) where T : class;
        Task<T> GetAsync<T>(object id) where T : class;
        Task<T> GetAsync<T>(CancellationToken cancellationToken, object id) where T : class;

        T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class;
        T First<T>(Expression<Func<T, bool>> predicate) where T : class;
        
        Task<T[]> GetManyAsync<T, TId>(params TId[] ids) where T : class, IHasId<TId>;
        Task<T[]> GetManyAsync<T, TId>(CancellationToken cancellationToken, params TId[] ids) where T : class, IHasId<TId>;
        Task<T[]> GetManyAsync<T>(params Guid[] ids) where T : class, IHasId<Guid>;
        Task<T[]> GetManyAsync<T>(CancellationToken cancellationToken, params Guid[] ids) where T : class, IHasId<Guid>;

        Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken)) where T : class;
        Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken)) where T : class;

        T Find<T>(object[] id) where T : class;
        T Find<T>(object id) where T : class;
        Task<T> FindAsync<T>(params object[] id) where T : class;
        Task<T> FindAsync<T>(CancellationToken cancellationToken, params object[] id) where T : class;
        Task<T> FindAsync<T>(object id) where T : class;
        Task<T> FindAsync<T>(CancellationToken cancellationToken, object id) where T : class;

        IQueryable<T> FindAll<T>() where T : class;
        Task<T[]> FindAllAsync<T>(CancellationToken cancellationToken = default(CancellationToken)) where T : class;
        IEnumerable<T> FindAllWithAdded<T>() where T : class;

        Task<T[]> FindManyAsync<T, TId>(params TId[] ids) where T : class, IHasId<TId>;
        Task<T[]> FindManyAsync<T, TId>(CancellationToken cancellationToken, params TId[] ids) where T : class, IHasId<TId>;
        Task<T[]> FindManyAsync<T>(params Guid[] ids) where T : class, IHasId<Guid>;
        Task<T[]> FindManyAsync<T>(CancellationToken cancellationToken, params Guid[] ids) where T : class, IHasId<Guid>;

        IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class;
        IEnumerable<T> WhereWithAdded<T>(Expression<Func<T, bool>> predicate) where T : class;
        
        IEnumerable<T> GetEntities<T>(params EntityState[] entityStates) where T : class;
        EntityState GetEntityState<T>(T entity) where T : class;
        void SetEntityState<T>(T entity, EntityState state) where T : class;
        bool IsAttached<T>(T entity) where T : class;
    }
}
