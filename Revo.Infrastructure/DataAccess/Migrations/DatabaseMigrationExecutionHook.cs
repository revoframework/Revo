using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.DataAccess.Migrations;

public class DatabaseMigrationExecutionHook(
    ILocalCommandBus localCommandBus,
    IDatabaseMigrationsConfiguration configuration,
    IEnvironment environment) : IStartupTask
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (configuration.ApplyMigrationsUponStartup == true
            || (environment.IsDevelopment && configuration.ApplyMigrationsUponStartup != false))
        {
            await localCommandBus.SendAsync(new ExecuteDatabaseMigrationsCommand(), cancellationToken);
        }
    }
}
