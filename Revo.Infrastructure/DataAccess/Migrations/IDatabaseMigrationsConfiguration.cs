using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationsConfiguration : IDatabaseMigrationExecutionOptions
    {
        bool? ApplyMigrationsUponStartup { get; }
        IDatabaseMigrationScripter OverrideDatabaseMigrationScripter { get; }
        DatabaseMigrationSelectorOptions MigrationSelectorOptions { get; }
    }
}