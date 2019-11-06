using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public struct PendingModuleMigration
    {
        public DatabaseMigrationSpecifier Specifier { get; set; }
        public IReadOnlyCollection<IDatabaseMigration> Migrations { get; set; }
        public IDatabaseMigrationProvider Provider { get; set; }
    }
}