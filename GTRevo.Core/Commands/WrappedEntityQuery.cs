using System.Web.OData.Query;

namespace GTRevo.Commands
{
    public class WrappedEntityQuery<TEntity, TWrapper> : IWrappedEntityQuery<TEntity, TWrapper>
    {
        public ODataQueryOptions<TEntity> QueryOptions { get; set; }
    }
}
