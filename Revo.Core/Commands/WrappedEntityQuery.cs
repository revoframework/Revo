using System.Web.OData.Query;

namespace Revo.Core.Commands
{
    public class WrappedEntityQuery<TEntity, TWrapper> : IWrappedEntityQuery<TEntity, TWrapper>
    {
        public ODataQueryOptions<TEntity> QueryOptions { get; set; }
    }
}
