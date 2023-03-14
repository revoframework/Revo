using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;

namespace Revo.Infrastructure.Repositories
{
    public interface IQueryableAggregateStore : IAggregateStore
    {
        Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
        Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;

        IQueryable<T> FindAll<T>() where T : class, IAggregateRoot, IQueryableEntity;
        Task<T[]> FindAllAsync<T>() where T : class, IAggregateRoot, IQueryableEntity;
        Task<T[]> FindAllAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
        IAsyncQueryableResolver GetQueryableResolver<T>() where T : class, IAggregateRoot, IQueryableEntity;

        IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
    }
}
