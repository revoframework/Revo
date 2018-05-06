using System.Web.OData.Query;
using Revo.Core.Commands;

namespace Revo.Platforms.AspNet.IO.OData
{
    public interface IWrappedEntityQuery<TEntity, TWrapper> : IQuery<TWrapper>
    {
        ODataQueryOptions<TEntity> QueryOptions { get; }
    }
}
