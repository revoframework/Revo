using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Revo.Domain.Entities;

namespace Revo.Infrastructure.Domain;

public sealed class EntityTypeManagerHostedLifecycleService(IEntityTypeManager entityTypeManager) : IHostedLifecycleService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        entityTypeManager.ClearCache();
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
