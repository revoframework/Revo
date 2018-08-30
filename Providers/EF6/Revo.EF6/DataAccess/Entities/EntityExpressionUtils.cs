using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;

namespace Revo.EF6.DataAccess.Entities
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
    }
}
