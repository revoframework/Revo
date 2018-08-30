using System.Web.OData.Query;

namespace Revo.AspNet.IO.OData
{
    public class WrappedEntityQuery<TEntity, TWrapper> : IWrappedEntityQuery<TEntity, TWrapper>
    {
        public ODataQueryOptions<TEntity> QueryOptions { get; set; }
    }
}
