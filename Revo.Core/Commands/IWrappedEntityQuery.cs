using System.Web.OData.Query;

namespace Revo.Core.Commands
{
    public interface IWrappedEntityQuery<TEntity, TWrapper> : IQuery<TWrapper>
    {
        ODataQueryOptions<TEntity> QueryOptions { get; }
    }
}
