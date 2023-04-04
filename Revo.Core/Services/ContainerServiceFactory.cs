using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Services;

public class ContainerServiceFactory<T>(IServiceProvider serviceProvider)
    : IServiceFactory<T>
{
    public T GetRequiredService()
    {
        return serviceProvider.GetRequiredService<T>();    
    }

    public T GetService()
    {
        return serviceProvider.GetService<T>();
    }

    public IEnumerable<T> GetServices()
    {
        return serviceProvider.GetServices<T>();
    }
}