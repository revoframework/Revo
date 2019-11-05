using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationExecutionHook : IApplicationConfigurer
    {
        private readonly IDatabaseMigrationExecutor executor;
        private readonly DatabaseMigrationsConfiguration configuration;
        private readonly IEnvironment environment;

        public DatabaseMigrationExecutionHook(IDatabaseMigrationExecutor executor,
            DatabaseMigrationsConfiguration configuration, IEnvironment environment)
        {
            this.executor = executor;
            this.configuration = configuration;
            this.environment = environment;
        }

        public void Configure()
        {
            if (configuration.ApplyMigrationsUponStartup == true
                || (environment.IsDevelopment && configuration.ApplyMigrationsUponStartup != false))
            {
                Task.Run(async () => await executor.ExecuteAsync()).Wait();
            }
        }
    }
}