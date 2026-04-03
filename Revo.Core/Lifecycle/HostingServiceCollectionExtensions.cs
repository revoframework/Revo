using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Lifecycle;

public static class HostingServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="T"/> as scoped and hosts it via <see cref="ScopedHostedService{T}"/>.
    /// </summary>
    public static IServiceCollection AddScopedHostedService<T>(this IServiceCollection services)
        where T : class, IStartupTask
    {
        services.AddScoped<T>();
        services.AddHostedService<ScopedHostedService<T>>();
        return services;
    }

    /// <summary>
    /// Registers <typeparamref name="T"/> as scoped and runs it from <see cref="ScopedHostedLifecycleStartingService{T}"/>
    /// during host <c>StartingAsync</c> (before other lifecycle <c>StartedAsync</c> work such as DB initialization).
    /// </summary>
    public static IServiceCollection AddScopedHostedLifecycleStartingService<T>(this IServiceCollection services)
        where T : class, IStartupTask
    {
        services.AddScoped<T>();
        services.AddHostedService<ScopedHostedLifecycleStartingService<T>>();
        return services;
    }
}
