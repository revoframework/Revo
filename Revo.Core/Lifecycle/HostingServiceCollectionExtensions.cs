using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Lifecycle;

/// <summary>
/// Registrations of the work executed upon application startup.
/// </summary>
/// <remarks>
/// The framework's startup work is divided by the phases of <see cref="Microsoft.Extensions.Hosting.IHostedLifecycleService"/>,
/// so that it does not depend on the order in which the hosted services were registered:
/// <list type="number">
/// <item><c>StartingAsync</c> - database schema (migrations).</item>
/// <item><c>StartAsync</c> - in-memory state of the framework (entity type cache, saga configuration, permission
/// index...) and the background job workers.</item>
/// <item><c>StartedAsync</c> - work processing the data, which may need any of the above: database initializers,
/// catch-up of the async event queues, message bus subscriptions.</item>
/// </list>
/// </remarks>
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
