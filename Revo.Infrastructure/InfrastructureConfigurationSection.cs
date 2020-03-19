using Revo.Core.Configuration;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Tenancy;

namespace Revo.Infrastructure
{
    public class InfrastructureConfigurationSection : IRevoConfigurationSection
    {
        public IAsyncEventPipelineConfiguration AsyncEventPipeline { get; set; } = new AsyncEventPipelineConfiguration();
        public DatabaseMigrationsConfiguration DatabaseMigrations { get; set; } = new DatabaseMigrationsConfiguration();
        public bool EnableSagas { get; set; } = true;
        public TenancyConfiguration Tenancy { get; set; } = new TenancyConfiguration();
    }
}
