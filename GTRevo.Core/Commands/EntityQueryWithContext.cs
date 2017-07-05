using System.Linq;

namespace GTRevo.Core.Commands
{
    public class EntityQueryWithContext<T> : QueryWithContext<IQueryable<T>>, IEntityQuery<T>
    {
    }
}
