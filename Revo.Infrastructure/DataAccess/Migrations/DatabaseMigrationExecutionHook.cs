using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationExecutionHook : IApplicationStartingListener
    {
        private readonly ILocalCommandBus localCommandBus;
        private readonly IDatabaseMigrationsConfiguration configuration;
        private readonly IEnvironment environment;

        public DatabaseMigrationExecutionHook(ILocalCommandBus localCommandBus,
            IDatabaseMigrationsConfiguration configuration,
            IEnvironment environment)
        {
            this.localCommandBus = localCommandBus;
            this.configuration = configuration;
            this.environment = environment;
        }

        public void OnApplicationStarting()
        {
            if (configuration.ApplyMigrationsUponStartup == true
                || (environment.IsDevelopment && configuration.ApplyMigrationsUponStartup != false))
            {
                Task.Run(async () => await localCommandBus
                    .SendAsync(new ExecuteDatabaseMigrationsCommand())).Wait();
            }
        }
    }
}