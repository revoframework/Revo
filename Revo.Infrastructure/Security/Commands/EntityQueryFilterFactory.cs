using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Infrastructure.Security.Commands;

public class EntityQueryFilterFactory : IEntityQueryFilterFactory
{
    private readonly IServiceProvider serviceProvider;

    public EntityQueryFilterFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IEnumerable<IEntityQueryFilter<T>> GetEntityQueryFilters<T>()
    {
        return serviceProvider.GetServices<IEntityQueryFilter<T>>();
    }
}
