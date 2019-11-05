using System;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationRecord
    {
        Guid Id { get; }
        DateTimeOffset TimeApplied { get; }
        string ModuleName { get; }
        DatabaseVersion Version { get; }
        string Checksum { get; }
        string FileName { get; }
    }
}