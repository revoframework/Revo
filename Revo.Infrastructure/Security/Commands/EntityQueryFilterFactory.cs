using System.Collections.Generic;
using Revo.Core.Services;

namespace Revo.Infrastructure.Security.Commands;

public class EntityQueryFilterFactory : IEntityQueryFilterFactory
{
    private readonly IVariantServiceResolver serviceResolver;

    public EntityQueryFilterFactory(IVariantServiceResolver serviceResolver)
    {
        this.serviceResolver = serviceResolver;
    }

    public IEnumerable<IEntityQueryFilter<T>> GetEntityQueryFilters<T>()
    {
        // variant resolution, so that filters for base entity types apply to derived entity types as well
        return serviceResolver.GetServices<IEntityQueryFilter<T>>();
    }
}
