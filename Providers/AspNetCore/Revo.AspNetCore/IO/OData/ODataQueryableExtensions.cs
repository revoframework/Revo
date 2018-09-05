using System.Linq;
using Microsoft.AspNet.OData.Query;

namespace Revo.AspNetCore.IO.OData
{
    public static class ODataQueryableExtensions
    {
        public static IQueryable<T> ApplyOptions<T>(this IQueryable<T> queryable, ODataQueryOptions<T> options)
        {
            return (IQueryable<T>)options.ApplyTo(queryable);
        }

        public static IQueryable<T> ApplyOptions<T>(this IQueryable<T> queryable, ODataQueryOptions<T> options, ODataQuerySettings querySettings)
        {
            return (IQueryable<T>)options.ApplyTo(queryable, querySettings);
        }
    }
}
