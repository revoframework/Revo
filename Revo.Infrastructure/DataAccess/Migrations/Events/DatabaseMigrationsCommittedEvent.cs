using System.Collections.Immutable;
using Revo.Core.Events;

namespace Revo.Infrastructure.DataAccess.Migrations.Events
{
    public class DatabaseMigrationsCommittedEvent : IEvent
    {
        public DatabaseMigrationsCommittedEvent(ImmutableArray<DatabaseMigrationInfo> migrations)
        {
            Migrations = migrations;
        }

        public ImmutableArray<DatabaseMigrationInfo> Migrations { get; }
    }
}