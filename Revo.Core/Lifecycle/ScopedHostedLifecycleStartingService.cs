using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Revo.Core.Lifecycle;

/// <summary>
/// Runs <typeparamref name="T"/> in a dedicated scope during <see cref="IHostedLifecycleService.StartingAsync"/>,
/// before <see cref="IHostedLifecycleService.StartedAsync"/> and <see cref="IHostedService.StartAsync"/>.
/// </summary>
public sealed class ScopedHostedLifecycleStartingService<T>(IServiceScopeFactory scopeFactory) : IHostedLifecycleService
    where T : IStartupTask
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var task = scope.ServiceProvider.GetRequiredService<T>();
        await task.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
