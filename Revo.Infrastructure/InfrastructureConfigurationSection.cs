using Revo.Core.Configuration;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure
{
    public class InfrastructureConfigurationSection : IRevoConfigurationSection
    {
        public IAsyncEventPipelineConfiguration AsyncEventPipeline { get; set; } = new AsyncEventPipelineConfiguration();
        public DatabaseMigrationsConfiguration DatabaseMigrations { get; set; } = new DatabaseMigrationsConfiguration();
        public bool EnableSagas { get; set; } = true;
    }
}
