using System.Collections.Generic;
using Revo.Core.Core;

namespace Revo.Infrastructure.Security.Commands
{
    public class EntityQueryFilterFactory : IEntityQueryFilterFactory
    {
        private readonly IServiceLocator serviceLocator;

        public EntityQueryFilterFactory(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public IEnumerable<IEntityQueryFilter<T>> GetEntityQueryFilters<T>()
        {
            return serviceLocator.GetAll<IEntityQueryFilter<T>>();
        }
    }
}
