using System.Collections.Generic;

namespace Revo.Core.Services;

/// <summary>
/// Resolves services from the container, honoring generic variance (see <see cref="IVariantServiceResolver"/>).
/// </summary>
public class ContainerServiceFactory<T>(IVariantServiceResolver serviceResolver)
    : IServiceFactory<T>
{
    public T GetRequiredService()
    {
        return serviceResolver.GetRequiredService<T>();
    }

    public T GetService()
    {
        return serviceResolver.GetService<T>();
    }

    public IEnumerable<T> GetServices()
    {
        return serviceResolver.GetServices<T>();
    }
}
