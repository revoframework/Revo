using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Infrastructure.DataAccess.Migrations.Events;

namespace Revo.Infrastructure.DataAccess.Migrations.Providers
{
    public abstract class AdoNetStubDatabaseMigrationProvider : IDatabaseMigrationProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEventBus eventBus;
        private bool isInitialized = false;

        protected AdoNetStubDatabaseMigrationProvider(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }
        
        protected abstract IDatabaseMigrationScripter Scripter { get; }

        public async Task<IReadOnlyCollection<IDatabaseMigrationRecord>> GetMigrationHistoryAsync()
        {
            var dbConnection = await GetInitializedDbConnectionAsync();

            using (var dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Scripter.SelectMigrationRecordsSql;

                List<IDatabaseMigrationRecord> result = new List<IDatabaseMigrationRecord>();

                if (dbCommand is DbCommand dbCommandAsync)
                {
                    using (var reader = await dbCommandAsync.ExecuteReaderAsync())
                    {
                        var columnDict = Enumerable.Range(0, reader.FieldCount).ToDictionary(i => reader.GetName(i), i => i);
                        while (await reader.ReadAsync())
                        {
                            result.Add(ParseMigrationRecord(reader, columnDict));
                        }
                    }
                }
                else
                {
                    using (var reader = dbCommand.ExecuteReader())
                    {
                        var columnDict = Enumerable.Range(0, reader.FieldCount).ToDictionary(i => reader.GetName(i), i => i);
                        while (reader.Read())
                        {
                            result.Add(ParseMigrationRecord(reader, columnDict));
                        }
                    }
                }

                return result;
            }
        }
        
        public async Task ApplyMigrationsAsync(IReadOnlyCollection<IDatabaseMigration> migrations)
        {
            var dbConnection = await GetInitializedDbConnectionAsync();
            
            using (var transaction = dbConnection.BeginTransaction())
            {
                foreach (var migration in migrations)
                {
                    if (!(migration is SqlDatabaseMigration sqlMigration))
                    {
                        throw new DatabaseMigrationException($"Cannot apply non-SQL migration ({migration}) to database with ADO.NET (not supported)");
                    }

                    try
                    {
                        foreach (string sqlCommand in sqlMigration.SqlCommands)
                        {
                            Logger.Debug($"Executing database migration using ADO.NET provider: {migration}");
                            
                            await OnMigrationBeforeAppliedAsync(migration);

                            using (var dbCommand = dbConnection.CreateCommand())
                            {
                                dbCommand.CommandText = sqlCommand;
                                dbCommand.Transaction = transaction;

                                if (dbCommand is DbCommand dbCommandAsync)
                                {
                                    await dbCommandAsync.ExecuteNonQueryAsync();
                
                                }
                                else
                                {
                                    dbCommand.ExecuteNonQuery();
                                }
                            }

                            await InsertMigrationRecordAsync(dbConnection, transaction, migration);

                            Logger.Info($"Applied database migration using ADO.NET provider: {migration}");

                            await OnMigrationAppliedAsync(migration);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new DatabaseMigrationException($"Failed to apply migration ({migration}) to database", e);
                    }
                }
                
                try
                {
                    Logger.Debug($"Commiting {migrations.Count} database migrations using ADO.NET provider");
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    throw new DatabaseMigrationException($"Failed to commit {migrations.Count} applied migrations to database", e);
                }
                
                await OnMigrationsCommittedAsync(migrations);
            }
        }

        public bool SupportsMigration(IDatabaseMigration migration)
        {
            return migration is SqlDatabaseMigration;
        }

        public virtual string[] GetProviderEnvironmentTags()
        {
            if (Scripter.DatabaseTypeTag != null)
            {
                return new[] {Scripter.DatabaseTypeTag};
            }

            return new string[0];
        }

        public virtual void Dispose()
        {
        }

        protected abstract Task<IDbConnection> GetDbConnectionAsync();

        private async Task OnMigrationBeforeAppliedAsync(IDatabaseMigration migration)
        {
            var migrationInfo = new DatabaseMigrationInfo(migration);
            await eventBus.PublishAsync(
                EventMessageDraft.FromEvent(
                    new DatabaseMigrationBeforeAppliedEvent(migrationInfo)));
        }

        private async Task OnMigrationAppliedAsync(IDatabaseMigration migration)
        {
            var migrationInfo = new DatabaseMigrationInfo(migration);
            await eventBus.PublishAsync(
                EventMessageDraft.FromEvent(
                    new DatabaseMigrationAppliedEvent(migrationInfo)));
        }

        private async Task OnMigrationsCommittedAsync(IReadOnlyCollection<IDatabaseMigration> migrations)
        {
            var migrationInfos = migrations.Select(x => new DatabaseMigrationInfo(x)).ToImmutableArray();
            await eventBus.PublishAsync(
                EventMessageDraft.FromEvent(
                    new DatabaseMigrationsCommittedEvent(migrationInfos)));
        }
        
        private async Task<IDbConnection> GetInitializedDbConnectionAsync()
        {
            var dbConnection = await GetDbConnectionAsync();

            if (!isInitialized)
            {
                await CreateSchemaAsync(dbConnection);
                isInitialized = true;
            }

            return dbConnection;
        }

        private async Task CreateSchemaAsync(IDbConnection dbConnection)
        {
            bool exists;
            using (var selectDbCommand = dbConnection.CreateCommand())
            {
                selectDbCommand.CommandText = Scripter.SelectMigrationSchemaExistsSql;

                object existsResult;
                if (selectDbCommand is DbCommand selectDbCommandAsync)
                {
                    existsResult = await selectDbCommandAsync.ExecuteScalarAsync();
                }
                else
                {
                    existsResult = selectDbCommand.ExecuteScalar();
                }

                exists = existsResult is Boolean
                    ? (bool) existsResult
                    : (existsResult is Int64
                        ? ((Int64) existsResult) > 0
                        : ((Int32) existsResult) > 0);
            }
            
            if (!exists)
            {
                Logger.Debug($"Creating database migration history schema using ADO.NET provider");

                using (var createDbCommand = dbConnection.CreateCommand())
                {
                    createDbCommand.CommandText = Scripter.CreateMigrationSchemaSql;

                    if (createDbCommand is DbCommand createDbCommandAsync)
                    {
                        await createDbCommandAsync.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        createDbCommand.ExecuteNonQuery();
                    }
                }

                Logger.Debug($"Successfully created database migration history schema using ADO.NET provider");
            }
        }

        private IDatabaseMigrationRecord ParseMigrationRecord(IDataReader reader, Dictionary<string, int> columnDict)
        {
            return new DatabaseMigrationRecord(
                reader.GetGuid(columnDict["id"]),
                reader.GetDateTime(columnDict["time_applied"]),
                reader.GetString(columnDict["module_name"]),
                reader.IsDBNull(columnDict["version"]) ? null : DatabaseVersion.Parse(reader.GetString(columnDict["version"])),
                reader.GetString(columnDict["checksum"]),
                reader.IsDBNull(columnDict["file_name"]) ? null : reader.GetString(columnDict["file_name"]));
        }

        private async Task InsertMigrationRecordAsync(IDbConnection dbConnection, IDbTransaction transaction,
            IDatabaseMigration migration)
        {
            using (var dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Scripter.InsertMigrationRecordSql;
                dbCommand.Transaction = transaction;
                var param = dbCommand.CreateParameter();
                param.DbType = DbType.Guid;
                param.ParameterName = "Id";
                param.Value = Guid.NewGuid();
                dbCommand.Parameters.Add(param);

                param = dbCommand.CreateParameter();
                param.DbType = DbType.String;
                param.ParameterName = "Version";
                param.Value = (object)migration.Version?.ToString() ?? DBNull.Value;
                dbCommand.Parameters.Add(param);

                param = dbCommand.CreateParameter();
                param.DbType = DbType.String;
                param.ParameterName = "ModuleName";
                param.Value = migration.ModuleName;
                dbCommand.Parameters.Add(param);

                param = dbCommand.CreateParameter();
                param.DbType = DbType.String;
                param.ParameterName = "FileName";
                param.Value = (object)(migration as FileSqlDatabaseMigration)?.FileName ?? DBNull.Value;
                dbCommand.Parameters.Add(param);

                param = dbCommand.CreateParameter();
                param.DbType = DbType.String;
                param.ParameterName = "Checksum";
                param.Value = migration.Checksum;
                dbCommand.Parameters.Add(param);

                param = dbCommand.CreateParameter();
                param.DbType = DbType.DateTimeOffset;
                param.ParameterName = "TimeApplied";
                param.Value = Clock.Current.Now.UtcDateTime;
                dbCommand.Parameters.Add(param);

                Logger.Debug($"Inserting {migration} database migration record");

                if (dbCommand is DbCommand dbCommandAsync)
                {
                    await dbCommandAsync.ExecuteNonQueryAsync();
                }
                else
                {
                    dbCommand.ExecuteNonQuery();
                }
            }
        }
    }
}