using System.Web.OData.Query;

namespace Revo.Core.Commands
{
    public class WrappedEntityQueryWithContext<TEntity, TWrapper> : QueryWithContext<TWrapper>,
        IWrappedEntityQuery<TEntity, TWrapper>
    {
        public ODataQueryOptions<TEntity> QueryOptions { get; set; }
    }
}
