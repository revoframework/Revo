using System;

namespace Revo.Infrastructure.DataAccess.Migrations.Providers
{
    public class DatabaseMigrationRecord : IDatabaseMigrationRecord
    {
        public DatabaseMigrationRecord(Guid id, DateTimeOffset timeApplied, string moduleName, DatabaseVersion version, string checksum, string fileName)
        {
            Id = id;
            TimeApplied = timeApplied;
            ModuleName = moduleName;
            Version = version;
            Checksum = checksum;
            FileName = fileName;
        }

        public Guid Id { get; }
        public DateTimeOffset TimeApplied { get; }
        public string ModuleName { get; }
        public DatabaseVersion Version { get; }
        public string Checksum { get; }
        public string FileName { get; }
    }
}