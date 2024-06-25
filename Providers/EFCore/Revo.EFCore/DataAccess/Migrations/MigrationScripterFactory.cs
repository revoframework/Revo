using System.Collections.Generic;
using System.Linq;
using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.EFCore.DataAccess.Migrations
{
    public class MigrationScripterFactory : IMigrationScripterFactory
    {
        private static readonly Dictionary<string[], IDatabaseMigrationScripter> ProviderScripters = new Dictionary<string[], IDatabaseMigrationScripter>()
        {
            { new []{ "npgsql", "postgresql" }, new PgsqlMigrationScripter() },
            { new []{ "sqlserver" }, new MssqlMigrationScripter() },
            { new []{ "sqlite" }, new SqliteMigrationScripter() },
            { new []{ "firebird" }, new GenericSqlDatabaseMigrationScripter("firebird") },
            { new []{ "oracle" }, new GenericSqlDatabaseMigrationScripter("oracle") }
        };

        private readonly IDatabaseMigrationsConfiguration migrationsConfiguration;

        public MigrationScripterFactory(IDatabaseMigrationsConfiguration migrationsConfiguration)
        {
            this.migrationsConfiguration = migrationsConfiguration;
        }

        public IDatabaseMigrationScripter GetProviderScripter(IEFCoreDatabaseAccess databaseAccess)
        {
            if (migrationsConfiguration.OverrideDatabaseMigrationScripter != null)
            {
                return migrationsConfiguration.OverrideDatabaseMigrationScripter;
            }

            var providerName = databaseAccess.GetDbContext(EFCoreDatabaseAccess.DefaultSchemaSpace)
                .Database.ProviderName.ToLowerInvariant();
            return ProviderScripters.FirstOrDefault(x => x.Key.Any(keyword => providerName.Contains(keyword))).Value;
        }
    }
}