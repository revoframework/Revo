using System.Linq;

namespace Revo.Core.Commands
{
    public class EntityQueryWithContext<T> : QueryWithContext<IQueryable<T>>, IEntityQuery<T>
    {
    }
}
