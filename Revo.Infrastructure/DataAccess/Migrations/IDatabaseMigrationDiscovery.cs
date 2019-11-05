using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationDiscovery
    {
        IEnumerable<IDatabaseMigration> DiscoverMigrations();
    }
}