using Revo.Core.Events;

namespace Revo.Infrastructure.DataAccess.Migrations.Events
{
    public class DatabaseMigrationAppliedEvent : IEvent
    {
        public DatabaseMigrationAppliedEvent(DatabaseMigrationInfo migration)
        {
            Migration = migration;
        }

        public DatabaseMigrationInfo Migration { get; }
    }
}