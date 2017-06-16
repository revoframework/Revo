using System.Web.OData.Query;

namespace GTRevo.Commands
{
    public interface IWrappedEntityQuery<TEntity, TWrapper> : IQuery<TWrapper>
    {
        ODataQueryOptions<TEntity> QueryOptions { get; }
    }
}
