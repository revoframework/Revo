using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.OData.Query;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Revo.AspNet.IO.OData;
using Revo.Core.IO.OData;

namespace Revo.RavenDB.AspNetOData.IO.OData
{
    public class RavenQueryableToODataResultConverter : IQueryableToODataResultConverter
    {
        public static readonly ODataQuerySettings ODataQuerySettings = new ODataQuerySettings()
        {
            EnableConstantParameterization = false,
            HandleNullPropagation = HandleNullPropagationOption.False
        };

        public bool Supports(IQueryable queryable)
        {
            return queryable.GetType().IsConstructedGenericType &&
                   queryable.GetType().GetGenericTypeDefinition() == typeof(RavenQueryInspector<>);
        }

        public async Task<ODataResult<T>> ToListAsync<T>(IQueryable<T> queryable,
            ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken)
        {
            var queryableApplied = (IQueryable<T>) queryOptions.ApplyTo(queryable, ODataQuerySettings);
            return new ODataResult<T>(await queryableApplied.ToListAsync(cancellationToken));
        }

        public async Task<ODataResultWithCount<T>> ToListWithCountAsync<T>(IQueryable<T> queryable,
            ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken)
        {
            var queryableWithStats = ((IRavenQueryable<T>) queryable)
                .Statistics(out QueryStatistics queryStats);
            var queryableApplied = (IQueryable<T>)queryOptions.ApplyTo(queryableWithStats, ODataQuerySettings);

            return new ODataResultWithCount<T>(
                await queryableApplied.ToListAsync(cancellationToken),
                queryStats.TotalResults);
        }
    }
}
