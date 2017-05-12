using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.DataAccess.EF6
{
    public interface IReadRepository : IDisposable
    {
        T Get<T>(object[] id) where T : class;
        T Get<T>(object id) where T : class;
        Task<T> GetAsync<T>(params object[] id) where T : class;
        Task<T> GetAsync<T>(object id) where T : class;

        T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class;
        T First<T>(Expression<Func<T, bool>> predicate) where T : class;
        Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class;
        Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        IQueryable<T> FindAll<T>() where T : class;
        Task<IList<T>> FindAllAsync<T>() where T : class;
        IEnumerable<T> FindAllWithAdded<T>() where T : class;

        IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class;
        IEnumerable<T> WhereWithAdded<T>(Expression<Func<T, bool>> predicate) where T : class;

        IEnumerable<DbEntityEntry> Entries();
        DbEntityEntry Entry(object entity);
        DbEntityEntry<T> Entry<T>(T entity) where T : class;
        bool IsAttached<T>(T entity) where T : class;
    }
}
