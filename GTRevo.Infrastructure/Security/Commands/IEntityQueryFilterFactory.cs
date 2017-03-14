using System.Collections.Generic;

namespace GTRevo.Infrastructure.Security.Commands
{
    public interface IEntityQueryFilterFactory
    {
        IEnumerable<IEntityQueryFilter<T>> GetEntityQueryFilters<T>();
    }
}