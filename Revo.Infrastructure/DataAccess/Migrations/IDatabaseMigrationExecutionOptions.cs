using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationExecutionOptions
    {
        IReadOnlyCollection<DatabaseMigrationSpecifier> MigrateOnlySpecifiedModules { get; }
        string[] EnvironmentTags { get; }
    }
}