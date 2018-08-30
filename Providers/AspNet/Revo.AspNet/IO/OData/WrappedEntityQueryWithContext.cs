using System.Web.OData.Query;
using Revo.Core.Commands;

namespace Revo.AspNet.IO.OData
{
    public class WrappedEntityQueryWithContext<TEntity, TWrapper> : QueryWithContext<TWrapper>,
        IWrappedEntityQuery<TEntity, TWrapper>
    {
        public ODataQueryOptions<TEntity> QueryOptions { get; set; }
    }
}
