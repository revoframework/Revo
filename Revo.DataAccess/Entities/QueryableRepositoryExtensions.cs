using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.DataAccess.Entities
{
    public static class QueryableRepositoryExtensions
    {
        public static Task<bool> AnyAsync<T>(this IQueryable<T> queryable, IAsyncQueryableResolver repository,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.AnyAsync(queryable, cancellationToken);
        }

        public static Task<int> CountAsync<T>(this IQueryable<T> queryable, IAsyncQueryableResolver repository,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.CountAsync(queryable, cancellationToken);
        }

        public static IQueryable<T> Include<T, TProperty>(this IQueryable<T> queryable,
            IAsyncQueryableResolver repository,
            Expression<Func<T, TProperty>> navigationPropertyPath) where T : class
        {
            return repository.Include(queryable, navigationPropertyPath);
        }

        public static IQueryable<T> Include<T>(this IQueryable<T> queryable, IAsyncQueryableResolver repository,
            string navigationPropertyPath) where T : class
        {
            return repository.Include(queryable, navigationPropertyPath);
        }

        public static Task<long> LongCountAsync<T>(this IQueryable<T> queryable, IAsyncQueryableResolver repository,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.LongCountAsync(queryable, cancellationToken);
        }

        public static Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> queryable, IAsyncQueryableResolver repository,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.FirstOrDefaultAsync(queryable, cancellationToken);
        }

        public static Task<T> FirstAsync<T>(this IQueryable<T> queryable, IAsyncQueryableResolver repository,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.FirstAsync(queryable, cancellationToken);
        }

        public static Task<T[]> ToArrayAsync<T>(this IQueryable<T> queryable, IAsyncQueryableResolver repository,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.ToArrayAsync(queryable, cancellationToken);
        }

        public static Task<List<T>> ToListAsync<T>(this IQueryable<T> queryable, IAsyncQueryableResolver repository,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.ToListAsync(queryable, cancellationToken);
        }

        public static Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(this IQueryable<T> queryable, IAsyncQueryableResolver repository, Func<T, TKey> keySelector,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.ToDictionaryAsync(queryable, keySelector, cancellationToken);
        }

        public static Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(this IQueryable<T> queryable, IAsyncQueryableResolver repository, Func<T, TKey> keySelector,
            IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.ToDictionaryAsync(queryable, keySelector, comparer, cancellationToken);
        }

        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(this IQueryable<T> queryable,
            IAsyncQueryableResolver repository, Func<T, TKey> keySelector,
            Func<T, TElement> elementSelector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.ToDictionaryAsync(queryable, keySelector, elementSelector, cancellationToken);
        }

        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<T, TKey, TElement>(this IQueryable<T> queryable, IAsyncQueryableResolver repository,
            Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return repository.ToDictionaryAsync(queryable, keySelector, elementSelector, comparer, cancellationToken);
        }

        public static T GetById<T, TId>(this IQueryable<T> queryable, TId id)
            where T : IHasId<TId>
        {
            var lambda = EntityExpressionUtils.CreateIdPropertyEqualsConstExpression<T, TId>(id);

            T t = queryable.FirstOrDefault(lambda);
            RepositoryHelpers.ThrowIfGetFailed(t, id);

            return t;
        }

        public static async Task<T> GetByIdAsync<T, TId>(this IQueryable<T> queryable, IAsyncQueryableResolver repository,  TId id,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : IHasId<TId>
        {
            var lambda = EntityExpressionUtils.CreateIdPropertyEqualsConstExpression<T, TId>(id);

            T t = await queryable.Where(lambda).FirstOrDefaultAsync(repository, cancellationToken);
            RepositoryHelpers.ThrowIfGetFailed(t, id);

            return t;
        }
    }
}
