using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.OData.Query;
using Revo.Core.IO.OData;
using Revo.Platforms.AspNet.IO.OData;

namespace Revo.DataAccess.EF6.IO.OData
{
    public class EF6QueryableToODataResultConverter : IQueryableToODataResultConverter
    {
        public bool Supports(IQueryable queryable)
        {
            return queryable is IDbAsyncEnumerable;
        }

        public async Task<ODataResult<T>> ToListAsync<T>(IQueryable<T> queryable,
            ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken)
        {
            return new ODataResult<T>(
                await queryable
                    .ApplyOptions(queryOptions)
                    .ToListAsync(cancellationToken));
        }

        public async Task<ODataResultWithCount<T>> ToListWithCountAsync<T>(IQueryable<T> queryable,
            ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken)
        {
            var list = await queryable
                .ApplyOptions(queryOptions)
                .ToListAsync(cancellationToken);
            long count = await ((IQueryable<T>)queryOptions
                .ApplyTo(queryable, AllowedQueryOptions.Skip | AllowedQueryOptions.Top))
                .LongCountAsync(cancellationToken);
            
            return new ODataResultWithCount<T>(list, count);
        }
    }
}
