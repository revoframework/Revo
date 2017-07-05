using System.Web.OData.Query;

namespace GTRevo.Core.Commands
{
    public class WrappedEntityQuery<TEntity, TWrapper> : IWrappedEntityQuery<TEntity, TWrapper>
    {
        public ODataQueryOptions<TEntity> QueryOptions { get; set; }
    }
}
