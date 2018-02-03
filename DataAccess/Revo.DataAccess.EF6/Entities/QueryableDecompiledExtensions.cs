using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using DelegateDecompiler;

namespace Revo.DataAccess.EF6.Entities
{
    public static class QueryableDecompiledExtensions
    {
        public static IQueryable<T> IncludeDecompiled<T, TProperty>(this IQueryable<T> queryable,
            Expression<Func<T, TProperty>> path)
        {
            var decompiled = path.Compile().Decompile();
            var newLambdaType = typeof(Expression<>)
                .MakeGenericType(
                    typeof(Func<,>).MakeGenericType(typeof(T), decompiled.ReturnType));

            var includeMethod = typeof(QueryableExtensions).GetMethods()
                .Single(x => x.Name == "Include"
                             && x.GetParameters()[1].ParameterType != typeof(string))
                .MakeGenericMethod(typeof(IQueryable<T>), newLambdaType);

            return (IQueryable<T>)includeMethod
                .Invoke(null, new object[] { queryable, decompiled });
        }
    }
}
