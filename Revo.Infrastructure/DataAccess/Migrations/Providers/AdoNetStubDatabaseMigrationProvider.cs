using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Infrastructure.DataAccess.Migrations.Events;

namespace Revo.Infrastructure.DataAccess.Migrations.Providers
{
    public abstract class AdoNetStubDatabaseMigrationProvider : IDatabaseMigrationProvider
    {
        private readonly IEventBus eventBus;
        private readonly ILogger logger;

        private readonly List<IDatabaseMigration> uncommittedMigrations = new List<IDatabaseMigration>();
        
        private IDbTransaction dbTransaction;
        private bool isInitialized = false;

        protected AdoNetStubDatabaseMigrationProvider(IEventBus eventBus, ILogger logger)
        {
            this.eventBus = eventBus;
            this.logger = logger;
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
            
            try
            {
                foreach (var migration in migrations)
                {
                    if (!(migration is SqlDatabaseMigration sqlMigration))
                    {
                        throw new DatabaseMigrationException(
                            $"Cannot apply non-SQL migration ({migration}) to database with ADO.NET (not supported)");
                    }
                    
                    switch (migration.TransactionMode)
                    {
                        case DatabaseMigrationTransactionMode.Default:
                            if (dbTransaction == null)
                            {
                                dbTransaction = await BeginDbTransactionAsync(dbConnection);
                            }
                            break;
                        case DatabaseMigrationTransactionMode.Isolated:
                            if (dbTransaction != null)
                            {
                                await CommitTransactionAsync();
                            }

                            dbTransaction = await BeginDbTransactionAsync(dbConnection);
                            break;
                        case DatabaseMigrationTransactionMode.WithoutTransaction:
                            if (dbTransaction != null)
                            {
                                await CommitTransactionAsync();
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    try
                    {
                        logger.LogInformation($"Executing database migration using ADO.NET provider: {migration}");

                        await OnMigrationBeforeAppliedAsync(migration);

                        uncommittedMigrations.Add(migration);

                        foreach (string sqlCommand in sqlMigration.SqlCommands)
                        {
                            using (var dbCommand = dbConnection.CreateCommand())
                            {
                                dbCommand.CommandText = sqlCommand;
                                dbCommand.Transaction = dbTransaction;

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

                        await InsertMigrationRecordAsync(migration);

                        logger.LogInformation($"Applied database migration using ADO.NET provider: {migration}");

                        await OnMigrationAppliedAsync(migration);

                        if (migration.TransactionMode == DatabaseMigrationTransactionMode.Isolated)
                        {
                            await CommitTransactionAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        throw new DatabaseMigrationException($"Failed to apply migration ({migration}) to database", e);
                    }
                }

                if (uncommittedMigrations.Count > 0)
                {
                    await CommitTransactionAsync();
                }
            }
            finally
            {
                if (dbTransaction != null)
                {
                    await RollbackDbTransactionAsync(dbTransaction);
                    dbTransaction = null;
                }

                uncommittedMigrations.Clear();
            }
        }

        private async Task CommitTransactionAsync()
        {
            try
            {
                logger.LogDebug($"Commiting {uncommittedMigrations.Count} database migrations using ADO.NET provider");
                await CommitDbTransactionAsync(dbTransaction);
                dbTransaction = null;
            }
            catch (Exception e)
            {
                throw new DatabaseMigrationException($"Failed to commit {uncommittedMigrations.Count} applied migrations to database", e);
            }

            await OnMigrationsCommittedAsync(uncommittedMigrations);
            uncommittedMigrations.Clear();
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

        protected virtual Task<IDbTransaction> BeginDbTransactionAsync(IDbConnection dbConnection)
        {
            return Task.FromResult(dbConnection.BeginTransaction());
        }

        protected virtual Task CommitDbTransactionAsync(IDbTransaction dbTransaction)
        {
            dbTransaction.Commit();
            return Task.CompletedTask;
        }

        protected virtual Task RollbackDbTransactionAsync(IDbTransaction dbTransaction)
        {
            dbTransaction.Rollback();
            return Task.CompletedTask;
        }

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
                logger.LogDebug($"Creating database migration history schema using ADO.NET provider");

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

                logger.LogDebug($"Successfully created database migration history schema using ADO.NET provider");
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

        private async Task InsertMigrationRecordAsync(IDatabaseMigration migration)
        {
            using (var dbCommand = dbTransaction.Connection.CreateCommand())
            {
                dbCommand.CommandText = Scripter.InsertMigrationRecordSql;
                dbCommand.Transaction = dbTransaction;
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
                param.Value = Clock.Current.UtcNow.UtcDateTime;
                dbCommand.Parameters.Add(param);

                logger.LogDebug($"Inserting {migration} database migration record");

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