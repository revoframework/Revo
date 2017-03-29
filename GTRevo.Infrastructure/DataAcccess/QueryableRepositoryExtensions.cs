using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using GTRevo.DataAccess.EF6;
using GTRevo.Infrastructure.Domain;

namespace GTRevo.Infrastructure.DataAcccess
{
    public static class QueryableRepositoryExtensions
    {
        public static T GetById<T>(this IQueryable<T> queryable, Guid id)
            where T : IEntityBase
        {
            T t = queryable.FirstOrDefault(x => x.Id == id);
            RepositoryHelpers.ThrowIfGetFailed(t, id);

            return t;
        }
        public static async Task<T> GetByIdAsync<T, TId>(this IQueryable<T> queryable, TId id)
            where T : IHasId<TId>
        {
            Expression idValueExpression = Expression.Constant(id);
            PropertyInfo idProperty = typeof(T).GetProperty("Id");
            ParameterExpression xParameterExpression = Expression.Parameter(typeof(T), "x");
            Expression idPropertyExpression = Expression.Property(xParameterExpression, idProperty);
            Expression lambdaBody = Expression.Equal(idPropertyExpression, idValueExpression);
            Expression<Func<T, bool>> lambda =
                (Expression<Func<T, bool>>) Expression.Lambda(lambdaBody, xParameterExpression);

            T t = await queryable.FirstOrDefaultAsync(lambda);
            RepositoryHelpers.ThrowIfGetFailed(t, id);

            return t;
        }

        public static Expression<Func<T, TId>> CreateGetIdPropertyExpression<T, TId>()
            where T : IHasId<TId>
        {
            PropertyInfo idProperty = typeof(T).GetProperty(nameof(IHasId<TId>.Id));
            ParameterExpression xParameterExpression = Expression.Parameter(typeof(T), "x");
            Expression idPropertyExpression = Expression.Property(xParameterExpression, idProperty);
            Expression<Func<T, TId>> lambda =
                (Expression<Func<T, TId>>)Expression.Lambda(idPropertyExpression, xParameterExpression);
            return lambda;
        }
    }
}
