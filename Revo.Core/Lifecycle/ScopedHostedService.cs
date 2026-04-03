using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Revo.Core.Lifecycle;

/// <summary>
/// Runs <typeparamref name="T"/> once in a dedicated service scope when the host starts.
/// </summary>
public sealed class ScopedHostedService<T>(IServiceScopeFactory scopeFactory) : IHostedService
    where T : IStartupTask
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var task = scope.ServiceProvider.GetRequiredService<T>();
        await task.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
