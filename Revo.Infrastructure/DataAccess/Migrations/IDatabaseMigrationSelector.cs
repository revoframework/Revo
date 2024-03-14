using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationSelector
    {
        Task<IReadOnlyCollection<SelectedModuleMigrations>> SelectMigrationsAsync(IDatabaseMigrationProvider migrationProvider,
            DatabaseMigrationSpecifier[] modules, string[] tags);
    }
}