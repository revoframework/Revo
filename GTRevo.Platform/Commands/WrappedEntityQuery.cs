using System.Linq;
using System.Web.OData.Query;

namespace GTRevo.Platform.Commands
{
    public class WrappedEntityQuery<TEntity, TWrapper> : IWrappedEntityQuery<TEntity, TWrapper>
    {
        public ODataQueryOptions<TEntity> QueryOptions { get; set; }
    }
}
