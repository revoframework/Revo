using System.Linq;
using System.Web.OData.Query;

namespace Revo.Platforms.AspNet.IO.OData
{
    public static class ODataQueryableExtensions
    {
        public static IQueryable<T> ApplyOptions<T>(this IQueryable<T> queryable, ODataQueryOptions<T> options)
        {
            return (IQueryable<T>)options.ApplyTo(queryable);
        }
    }
}
