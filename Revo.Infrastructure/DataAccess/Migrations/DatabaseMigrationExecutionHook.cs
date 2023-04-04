using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Revo.Core.Commands;
using Revo.Core.Core;
namespace Revo.Infrastructure.DataAccess.Migrations;

public class DatabaseMigrationExecutionHook(
    ILocalCommandBus localCommandBus,
    IDatabaseMigrationsConfiguration configuration,
    IEnvironment environment) : IHostedLifecycleService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        if (configuration.ApplyMigrationsUponStartup == true
            || (environment.IsDevelopment && configuration.ApplyMigrationsUponStartup != false))
        {
            await localCommandBus.SendAsync(new ExecuteDatabaseMigrationsCommand(), cancellationToken);
        }
    }

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
