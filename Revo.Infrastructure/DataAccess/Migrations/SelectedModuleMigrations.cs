using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class SelectedModuleMigrations
    {
        public SelectedModuleMigrations(DatabaseMigrationSpecifier specifier, IReadOnlyCollection<IDatabaseMigration> migrations)
        {
            Specifier = specifier;
            Migrations = migrations;
        }

        public DatabaseMigrationSpecifier Specifier { get; set; }
        public IReadOnlyCollection<IDatabaseMigration> Migrations { get; set; }
    }
}