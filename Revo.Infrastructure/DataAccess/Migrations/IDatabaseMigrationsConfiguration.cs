using System.Collections.Generic;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationsConfiguration
    {
        IReadOnlyCollection<DatabaseMigrationSpecifier> MigrateOnlySpecifiedModules { get; }
        bool? ApplyMigrationsUponStartup { get; }
        string[] EnvironmentTags { get; }
        IReadOnlyCollection<FileDatabaseMigrationDiscoveryPath> ScannedFilePaths { get; }
        IDatabaseMigrationScripter OverrideDatabaseMigrationScripter { get; }
        IReadOnlyCollection<ResourceDatabaseMigrationDiscoveryAssembly> ScannedAssemblies { get; }
    }
}