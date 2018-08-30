using System.Collections.Generic;

namespace Revo.Infrastructure.Security.Commands
{
    public interface IEntityQueryFilterFactory
    {
        IEnumerable<IEntityQueryFilter<T>> GetEntityQueryFilters<T>();
    }
}