using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.EF6.DataAccess.Migrations
{
    public interface IMigrationScripterFactory
    {
        IDatabaseMigrationScripter GetProviderScripter(EF6ConnectionConfiguration connectionConfiguration);
    }
}