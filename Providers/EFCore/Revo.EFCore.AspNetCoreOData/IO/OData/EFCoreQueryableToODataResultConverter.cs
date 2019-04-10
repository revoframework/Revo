using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Revo.AspNetCore.IO.OData;
using Revo.Core.IO.OData;

namespace Revo.EFCore.AspNetCoreOData.IO.OData
{
    public class EFCoreQueryableToODataResultConverter : IQueryableToODataResultConverter
    {
        private readonly bool disableAsyncQueryableResolution;
        private readonly ODataQuerySettings oDataQuerySettings;

        public EFCoreQueryableToODataResultConverter(bool disableAsyncQueryableResolution, ODataQuerySettings oDataQuerySettings)
        {
            this.disableAsyncQueryableResolution = disableAsyncQueryableResolution;
            this.oDataQuerySettings = oDataQuerySettings;
        }

        public bool Supports(IQueryable queryable)
        {
            return queryable.GetType().GetInterfaces().Any(x =>
                       x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
                   && queryable.Provider is IAsyncQueryProvider;
        }

        public async Task<ODataResult<T>> ToListAsync<T>(IQueryable<T> queryable,
            ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken)
        {
            if (disableAsyncQueryableResolution) 
            {
                return new ODataResult<T>(queryable
                    .ApplyOptions(queryOptions, oDataQuerySettings)
                    .ToList());
            }
            else
            {
                return new ODataResult<T>(
                    await queryable
                        .ApplyOptions(queryOptions, oDataQuerySettings)
                        .ToListAsync(cancellationToken));
            }
        }

        public async Task<ODataResultWithCount<T>> ToListWithCountAsync<T>(IQueryable<T> queryable,
            ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken)
        {
            long count;
            List<T> list;

            if (disableAsyncQueryableResolution)
            {
                list = queryable
                    .ApplyOptions(queryOptions, oDataQuerySettings)
                    .ToList();
                count = ((IQueryable<T>)queryOptions
                        .ApplyTo(queryable, oDataQuerySettings, AllowedQueryOptions.Skip | AllowedQueryOptions.Top))
                    .LongCount();
            }
            else
            {
                list = await queryable
                    .ApplyOptions(queryOptions, oDataQuerySettings)
                    .ToListAsync(cancellationToken);
                count = await ((IQueryable<T>)queryOptions
                        .ApplyTo(queryable, oDataQuerySettings, AllowedQueryOptions.Skip | AllowedQueryOptions.Top))
                    .LongCountAsync(cancellationToken);
            }

            return new ODataResultWithCount<T>(list, count);
        }
    }
}
