using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationSelector
    {
        Task<IReadOnlyCollection<IDatabaseMigration>> SelectMigrationsAsync(string moduleName, string[] tags,
            DatabaseVersion targetVersion = null);
    }
}