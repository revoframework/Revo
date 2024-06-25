using System.Collections.Generic;
using Revo.Core.Core;

namespace Revo.Infrastructure.Security.Commands
{
    public class EntityQueryFilterFactory(IServiceLocator serviceLocator) : IEntityQueryFilterFactory
    {
        public IEnumerable<IEntityQueryFilter<T>> GetEntityQueryFilters<T>()
        {
            return serviceLocator.GetAll<IEntityQueryFilter<T>>();
        }
    }
}
