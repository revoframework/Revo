using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Revo.DataAccess.Entities
{
    public static class EntityExpressionUtils
    {
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

        public static Expression<Func<T, bool>> CreateIdPropertyEqualsConstExpression<T, TId>(TId id)
            where T : IHasId<TId>
        {
            Expression idValueExpression = Expression.Constant(id);
            PropertyInfo idProperty = typeof(T).GetProperty(nameof(IHasId<TId>.Id));
            ParameterExpression xParameterExpression = Expression.Parameter(typeof(T), "x");
            Expression idPropertyExpression = Expression.Property(xParameterExpression, idProperty);
            Expression lambdaBody = Expression.Equal(idPropertyExpression, idValueExpression);
            return (Expression<Func<T, bool>>)Expression.Lambda(lambdaBody, xParameterExpression);
        }
    }
}
