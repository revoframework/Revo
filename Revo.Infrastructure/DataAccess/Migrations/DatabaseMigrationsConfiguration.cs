using System.Collections.Generic;
using MoreLinq.Extensions;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationsConfiguration : IDatabaseMigrationsConfiguration
    {
        public List<DatabaseMigrationSpecifier> MigrateOnlySpecifiedModules { get; set; } = null;
        public bool? ApplyMigrationsUponStartup { get; set; }
        public string[] EnvironmentTags { get; set; } = new string[0];
        public HashSet<FileDatabaseMigrationDiscoveryPath> ScannedFilePaths { get; set; } = new HashSet<FileDatabaseMigrationDiscoveryPath>()
        {
            new FileDatabaseMigrationDiscoveryPath("Sql")
        };
        public IDatabaseMigrationScripter OverrideDatabaseMigrationScripter { get; set; }

        public HashSet<ResourceDatabaseMigrationDiscoveryAssembly> ScannedAssemblies { get; set; } =
            new HashSet<ResourceDatabaseMigrationDiscoveryAssembly>();

        public void AddScannedAssembly(params ResourceDatabaseMigrationDiscoveryAssembly[] assemblies)
        {
            assemblies.ForEach(x => ScannedAssemblies.Add(x));
        }

        public void AddScannedFilePaths(params FileDatabaseMigrationDiscoveryPath[] paths)
        {
            paths.ForEach(x => ScannedFilePaths.Add(x));
        }

        IReadOnlyCollection<DatabaseMigrationSpecifier> IDatabaseMigrationsConfiguration.
            MigrateOnlySpecifiedModules => MigrateOnlySpecifiedModules;
        IReadOnlyCollection<FileDatabaseMigrationDiscoveryPath> IDatabaseMigrationsConfiguration.
            ScannedFilePaths => ScannedFilePaths;
        IReadOnlyCollection<ResourceDatabaseMigrationDiscoveryAssembly> IDatabaseMigrationsConfiguration.
            ScannedAssemblies => ScannedAssemblies;
    }
}