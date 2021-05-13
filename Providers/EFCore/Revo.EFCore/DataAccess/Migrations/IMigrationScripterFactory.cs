using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.EFCore.DataAccess.Migrations
{
    public interface IMigrationScripterFactory
    {
        IDatabaseMigrationScripter GetProviderScripter(IEFCoreDatabaseAccess databaseAccess);
    }
}