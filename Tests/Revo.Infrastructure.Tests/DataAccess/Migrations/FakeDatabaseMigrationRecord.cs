using System;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.Infrastructure.Tests.DataAccess.Migrations
{
    public class FakeDatabaseMigrationRecord : IDatabaseMigrationRecord
    {
        public Guid Id { get; set; }
        public DateTimeOffset TimeApplied { get; set;  }
        public string ModuleName { get; set; }
        public DatabaseVersion Version { get; set; }
        public string Checksum { get; set; }
        public string FileName { get; set; }
    }
}