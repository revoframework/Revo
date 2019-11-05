using Microsoft.EntityFrameworkCore;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.EFCore.DataAccess.Migrations
{
    public interface IMigrationScripterFactory
    {
        IDatabaseMigrationScripter GetProviderScripter(DbContextOptions<DatabaseMigrationDbContext> contextOptions);
    }
}