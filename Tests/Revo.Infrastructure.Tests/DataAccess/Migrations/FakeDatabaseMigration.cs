using System.Collections.Generic;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.Infrastructure.Tests.DataAccess.Migrations
{
    public class FakeDatabaseMigration : IDatabaseMigration
    {
        public string ModuleName { get; set; }
        public bool IsBaseline { get; set; }
        public bool IsRepeatable { get; set; }
        public DatabaseVersion Version { get; set; }
        public IReadOnlyCollection<DatabaseMigrationSpecifier> Dependencies { get; set; }
        public string[][] Tags { get; set; }
        public string Description { get; set; }
        public string Checksum { get; set; }
    }
}