using System.Threading.Tasks;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationExecutionHook : IApplicationConfigurer
    {
        private readonly IDatabaseMigrationExecutor executor;

        public DatabaseMigrationExecutionHook(IDatabaseMigrationExecutor executor)
        {
            this.executor = executor;
        }

        public void Configure()
        {
            Task.Run(async () => await executor.ExecuteAsync()).Wait();
        }
    }
}