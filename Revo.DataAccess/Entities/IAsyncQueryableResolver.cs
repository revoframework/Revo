using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.DataAccess.Entities
{
    public interface IAsyncQueryableResolver
    {
        Task<bool> AnyAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> CountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken));
        IQueryable<T> Include<T, TProperty>(IQueryable<T> queryable, Expression<Func<T, TProperty>> navigationPropertyPath) where T : class;
        IQueryable<T> Include<T>(IQueryable<T> queryable, string navigationPropertyPath) where T : class;
        Task<long> LongCountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken));
        Task<T> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken));
        Task<T> FirstAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken));
        Task<T[]> ToArrayAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken));
        Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(IQueryable<T> queryable, Func<T, TKey> keySelector,
            CancellationToken cancellationToken = default(CancellationToken));
        Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(IQueryable<T> queryable, Func<T, TKey> keySelector,
            IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken));
        Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(IQueryable<T> queryable, Func<T, TKey> keySelector,
            Func<T, TElement> elementSelector, CancellationToken cancellationToken = default(CancellationToken));
        Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(IQueryable<T> queryable, Func<T, TKey> keySelector,
            Func<T, TElement> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken));
    }
}
