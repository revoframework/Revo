using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GTRevo.Infrastructure.Core.Domain;
using LinqKit;

namespace GTRevo.Infrastructure.Repositories
{
    public static class RepositoryExtensions
    {
        public static T AddIfNew<T, TKey>(this IRepository repository, Expression<Func<T, TKey>> keyExpression, T entity) where T : class, IQueryableEntity, IAggregateRoot
        {
            var entityParam = Expression.Constant(entity);
            var entityKeyExpression = Expression.Invoke(keyExpression, entityParam);

            var xParam = Expression.Parameter(typeof(T), "x");
            var xKeyExpression = Expression.Invoke(keyExpression, xParam);
            var comparisonExpression = Expression.Equal(entityKeyExpression, xKeyExpression);
            var whereExpression = Expression.Lambda<Func<T, bool>>(comparisonExpression, xParam);

            T old = repository.FirstOrDefault<T>(whereExpression.Expand()); // TODO optimize for ID keys
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

        public static T AddIfNew<T>(this IRepository repository, T entity) where T : class, IQueryableEntity, IAggregateRoot
        {
            return AddIfNew(repository, x => x.Id, entity);
        }

        public static List<T> AddIfNew<T, TKey>(this IRepository repository, Expression<Func<T, TKey>> keyExpression, params T[] entities) where T : class, IQueryableEntity, IAggregateRoot
        {
            List<T> result = new List<T>();
            foreach (T entity in entities)
            {
                result.Add(AddIfNew(repository, keyExpression, entity));
            }

            return result;
        }

        public static List<T> AddIfNew<T>(this IRepository repository, params T[] entities) where T : class, IQueryableEntity, IAggregateRoot
        {
            return AddIfNew(repository, x => x.Id, entities);
        }
    }
}
