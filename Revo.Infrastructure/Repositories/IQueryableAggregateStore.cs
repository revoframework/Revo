using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.Domain.Entities;

namespace Revo.Infrastructure.Repositories
{
    public interface IQueryableAggregateStore : IAggregateStore
    {
        T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
        T First<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
        Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
        Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;

        IQueryable<T> FindAll<T>() where T : class, IAggregateRoot, IQueryableEntity;
        Task<IList<T>> FindAllAsync<T>() where T : class, IAggregateRoot, IQueryableEntity;

        IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
    }
}
