using System.Collections.Generic;
using Ninject;

namespace Revo.Infrastructure.Security.Commands
{
    public class EntityQueryFilterFactory : IEntityQueryFilterFactory
    {
        private readonly StandardKernel kernel;

        public EntityQueryFilterFactory(StandardKernel kernel)
        {
            this.kernel = kernel;
        }

        public IEnumerable<IEntityQueryFilter<T>> GetEntityQueryFilters<T>()
        {
            return kernel.GetAll<IEntityQueryFilter<T>>();
        }
    }
}
