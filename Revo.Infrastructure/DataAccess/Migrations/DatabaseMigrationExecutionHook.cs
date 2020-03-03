using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationExecutionHook : IApplicationConfigurer
    {
        private readonly IDatabaseMigrationExecutor executor;
        private readonly IDatabaseMigrationsConfiguration configuration;
        private readonly IEnvironment environment;

        public DatabaseMigrationExecutionHook(IDatabaseMigrationExecutor executor,
            IDatabaseMigrationsConfiguration configuration, IEnvironment environment)
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