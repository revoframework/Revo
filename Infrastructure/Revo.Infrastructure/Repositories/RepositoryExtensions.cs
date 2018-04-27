using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LinqKit;
using Revo.Domain.Entities;

namespace Revo.Infrastructure.Repositories
{
    public static class RepositoryExtensions
    {
        public static async Task<T> AddIfNewAsync<T, TKey>(this IRepository repository, Expression<Func<T, TKey>> keyExpression, T entity) where T : class, IQueryableEntity, IAggregateRoot
        {
            var entityParam = Expression.Constant(entity);
            var entityKeyExpression = Expression.Invoke(keyExpression, entityParam);

            var xParam = Expression.Parameter(typeof(T), "x");
            var xKeyExpression = Expression.Invoke(keyExpression, xParam);
            var comparisonExpression = Expression.Equal(entityKeyExpression, xKeyExpression);
            var whereExpression = Expression.Lambda<Func<T, bool>>(comparisonExpression, xParam);

            T old = await repository.FirstOrDefaultAsync<T>(whereExpression.Expand());
            if (old == null)
            {
                repository.Add(entity);
                return entity;
            }
            else
            {
                return old;
            }
        }

        public static async Task<T> AddIfNewAsync<T>(this IRepository repository, T entity) where T : class, IAggregateRoot
        {
            T old = await repository.FindAsync<T>(entity.Id);
            if (old == null)
            {
                repository.Add(entity);
                return entity;
            }
            else
            {
                return old;
            }
        }

        public static async Task<List<T>> AddIfNewAsync<T, TKey>(this IRepository repository, Expression<Func<T, TKey>> keyExpression, params T[] entities) where T : class, IQueryableEntity, IAggregateRoot
        {
            List<T> result = new List<T>();
            foreach (T entity in entities)
            {
                result.Add(await AddIfNewAsync(repository, keyExpression, entity));
            }

            return result;
        }

        public static async Task<List<T>> AddIfNewAsync<T>(this IRepository repository, params T[] entities) where T : class, IQueryableEntity, IAggregateRoot
        {
            List<T> result = new List<T>();
            foreach (T entity in entities)
            {
                result.Add(await AddIfNewAsync(repository, entity));
            }

            return result;
        }
    }
}
