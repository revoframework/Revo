using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface ISqlDatabaseMigration : IDatabaseMigration
    {
        IReadOnlyCollection<string> SqlCommands { get; }
    }
}