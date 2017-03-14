using System.Linq;
using System.Web.OData.Query;

namespace GTRevo.Platform.Commands
{
    public class EntityQueryWithContext<T> : QueryWithContext<IQueryable<T>>, IEntityQuery<T>
    {
        public ODataQueryOptions<T> QueryOptions { get; set; }
    }
}
