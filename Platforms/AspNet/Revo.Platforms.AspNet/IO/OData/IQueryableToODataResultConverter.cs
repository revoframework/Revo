using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.OData.Query;
using Revo.Core.IO.OData;

namespace Revo.Platforms.AspNet.IO.OData
{
    public interface IQueryableToODataResultConverter
    {
        bool Supports(IQueryable queryable);
        Task<ODataResult<T>> ToListAsync<T>(IQueryable<T> queryable,
            ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken);
        Task<ODataResultWithCount<T>> ToListWithCountAsync<T>(IQueryable<T> queryable,
            ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken);
    }
}
