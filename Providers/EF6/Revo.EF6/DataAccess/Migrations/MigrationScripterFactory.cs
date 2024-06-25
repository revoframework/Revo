using System.Collections.Generic;
using System.Linq;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.EF6.DataAccess.Migrations
{
    public class MigrationScripterFactory : IMigrationScripterFactory
    {
        private static readonly Dictionary<string[], IDatabaseMigrationScripter> ProviderScripters = new Dictionary<string[], IDatabaseMigrationScripter>()
        {
            { new []{ "npgsql", "postgresql" }, new PgsqlMigrationScripter() },
            { new []{ "localdb" }, new MssqlMigrationScripter() },
            { new []{ "sqlite" }, new GenericSqlDatabaseMigrationScripter("sqlite") },
            { new []{ "firebird" }, new GenericSqlDatabaseMigrationScripter("firebird") },
            { new []{ "oracle" }, new GenericSqlDatabaseMigrationScripter("oracle") }
        };

        private readonly IDatabaseMigrationsConfiguration migrationsConfiguration;

        public MigrationScripterFactory(IDatabaseMigrationsConfiguration migrationsConfiguration)
        {
            this.migrationsConfiguration = migrationsConfiguration;
        }

        public IDatabaseMigrationScripter GetProviderScripter(EF6ConnectionConfiguration connectionConfiguration)
        {
            if (migrationsConfiguration.OverrideDatabaseMigrationScripter != null)
            {
                return migrationsConfiguration.OverrideDatabaseMigrationScripter;
            }

            string typeName = connectionConfiguration.ConnectionFactory.GetType().FullName?.ToLowerInvariant();
            if (typeName != null)
            {
                return ProviderScripters.FirstOrDefault(x => x.Key.Any(keyword => typeName.Contains(keyword))).Value;
            }

            return null;
        }
    }
}