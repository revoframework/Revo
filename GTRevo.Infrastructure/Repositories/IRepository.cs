using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Core.Domain;

namespace GTRevo.Infrastructure.Repositories
{
    public interface IRepository : IUnitOfWorkProvider, IDisposable
    {
        void Add<T>(T aggregate) where T : class, IAggregateRoot;

        T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
        T First<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
        Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
        Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;

        T Get<T>(Guid id) where T : class, IAggregateRoot;
        Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot;

        IQueryable<T> FindAll<T>() where T : class, IAggregateRoot, IQueryableEntity;
        Task<IList<T>> FindAllAsync<T>() where T : class, IAggregateRoot, IQueryableEntity;

        IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity;
        
        void Remove<T>(T aggregate) where T : class, IAggregateRoot;

        /// <summary>
        /// Saves the repository changes. Not needed when using unit of work.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Asynchronously save the repository changes. Not needed when using unit of work.
        /// </summary>
        Task SaveChangesAsync();
    }
}
