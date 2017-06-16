using System.Linq;

namespace GTRevo.Commands
{
    public class EntityQueryWithContext<T> : QueryWithContext<IQueryable<T>>, IEntityQuery<T>
    {
    }
}
