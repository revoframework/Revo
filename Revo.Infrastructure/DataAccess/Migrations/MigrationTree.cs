using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess.Migrations;

public class MigrationTree
{
    public List<MigrationTreeNode> Roots { get; } = new();
}