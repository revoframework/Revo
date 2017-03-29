using System.Web.OData.Query;

namespace GTRevo.Platform.Commands
{
    public interface IWrappedEntityQuery<TEntity, TWrapper> : IQuery<TWrapper>
    {
        ODataQueryOptions<TEntity> QueryOptions { get; }
    }
}
