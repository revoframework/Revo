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
        public static async Task<T> GetByIdAsync<T>(this IQueryable<T> queryable, Guid id)
            where T : IEntityBase
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
    }
}
