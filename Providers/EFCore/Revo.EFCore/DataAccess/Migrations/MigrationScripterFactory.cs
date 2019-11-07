﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        public IDatabaseMigrationScripter GetProviderScripter(DbContextOptions<DatabaseMigrationDbContext> contextOptions)
        {
            if (migrationsConfiguration.OverrideDatabaseMigrationScripter != null)
            {
                return migrationsConfiguration.OverrideDatabaseMigrationScripter;
            }

            var dbProviderExtension = contextOptions.Extensions.FirstOrDefault(x => x.Info.IsDatabaseProvider);
            string typeName = dbProviderExtension?.GetType().FullName?.ToLowerInvariant();
            if (typeName != null)
            {
                return ProviderScripters.FirstOrDefault(x => x.Key.Any(keyword => typeName.Contains(keyword))).Value;
            }

            return null;
        }
    }
}