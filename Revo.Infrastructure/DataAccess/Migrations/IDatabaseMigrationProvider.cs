using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationProvider : IDisposable
    {
        Task<IReadOnlyCollection<IDatabaseMigrationRecord>> GetMigrationHistoryAsync();
        Task ApplyMigrationsAsync(IReadOnlyCollection<IDatabaseMigration> migrations);
        bool SupportsMigration(IDatabaseMigration migration);
        string[] GetProviderEnvironmentTags();
    }
}