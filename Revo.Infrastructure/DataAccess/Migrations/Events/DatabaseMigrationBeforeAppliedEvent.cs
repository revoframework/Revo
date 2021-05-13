using Revo.Core.Events;

namespace Revo.Infrastructure.DataAccess.Migrations.Events
{
    public class DatabaseMigrationBeforeAppliedEvent : IEvent
    {
        public DatabaseMigrationBeforeAppliedEvent(DatabaseMigrationInfo migration)
        {
            Migration = migration;
        }

        public DatabaseMigrationInfo Migration { get; }
    }
}