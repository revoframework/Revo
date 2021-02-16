using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationNode
    {
        private readonly HashSet<DatabaseMigrationNode> parents = new HashSet<DatabaseMigrationNode>();
        private readonly HashSet<DatabaseMigrationNode> children = new HashSet<DatabaseMigrationNode>();

        public DatabaseMigrationNode(DatabaseMigrationSpecifier migration)
        {
            Migration = migration;
        }

        public IReadOnlyCollection<DatabaseMigrationNode> Parents => parents;
        public IReadOnlyCollection<DatabaseMigrationNode> Children => children;
        public DatabaseMigrationSpecifier Migration { get; }

        public void AddParent(DatabaseMigrationNode parent)
        {
            parents.Add(parent);
        }

        public void AddChild(DatabaseMigrationNode child)
        {
            children.Add(child);
        }
    }
}