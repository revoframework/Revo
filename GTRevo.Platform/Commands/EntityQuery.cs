using System.Web.OData.Query;

namespace GTRevo.Platform.Commands
{
    public class EntityQuery<T> : IEntityQuery<T>
    {
        public ODataQueryOptions<T> QueryOptions { get; set; }
    }
}
