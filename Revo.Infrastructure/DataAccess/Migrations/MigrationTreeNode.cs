using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class MigrationTreeNode
    {
        public IDatabaseMigration Migration { get; init; }
        public List<MigrationTreeNode> Dependencies { get; } = new();
    }
}