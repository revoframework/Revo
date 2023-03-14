using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Revo.Core.Events;
using Revo.EF6.DataAccess.Entities;
using Revo.EF6.DataAccess.Model;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.EF6.DataAccess.Migrations
{
    public class EF6DatabaseMigrationProvider : AdoNetStubDatabaseMigrationProvider
    {
        private readonly IEF6DatabaseAccess databaseAccess;

        public EF6DatabaseMigrationProvider(EF6ConnectionConfiguration connectionConfiguration,
            IMigrationScripterFactory scripterFactory,
            IEventBus eventBus, IEF6DatabaseAccess databaseAccess, ILogger logger) : base(eventBus, logger)
        {
            this.databaseAccess = databaseAccess;

            Scripter = scripterFactory.GetProviderScripter(connectionConfiguration);
        }

        protected override IDatabaseMigrationScripter Scripter { get; }

        protected override Task<IDbTransaction> BeginDbTransactionAsync(IDbConnection dbConnection)
        {
            var dbContext = databaseAccess.GetDbContext(EntityTypeDiscovery.DefaultSchemaSpace);
            var dbContextTransaction = dbContext.Database.BeginTransaction();
            return Task.FromResult<IDbTransaction>(dbContextTransaction.UnderlyingTransaction);
        }

        protected override Task CommitDbTransactionAsync(IDbTransaction dbTransaction)
        {
            var dbContext = databaseAccess.GetDbContext(EntityTypeDiscovery.DefaultSchemaSpace);
            if (dbContext.Database.CurrentTransaction == null
                || dbContext.Database.CurrentTransaction.UnderlyingTransaction != dbTransaction)
            {
                throw new ArgumentException("Invalid dbTransaction passed");
            }

            dbContext.Database.CurrentTransaction.Commit();
            return Task.CompletedTask;
        }

        protected override Task RollbackDbTransactionAsync(IDbTransaction dbTransaction)
        {
            var dbContext = databaseAccess.GetDbContext(EntityTypeDiscovery.DefaultSchemaSpace);
            if (dbContext.Database.CurrentTransaction == null
                || dbContext.Database.CurrentTransaction.UnderlyingTransaction != dbTransaction)
            {
                throw new ArgumentException("Invalid dbTransaction passed");
            }

            dbContext.Database.CurrentTransaction.Rollback();
            return Task.CompletedTask;
        }

        public override string[] GetProviderEnvironmentTags()
        {
            List<string> tags = new List<string>(base.GetProviderEnvironmentTags());
            tags.Add("ef6");

            return tags.ToArray();
        }

        protected override async Task<IDbConnection> GetDbConnectionAsync()
        {
            var dbContext = databaseAccess.GetDbContext(EntityTypeDiscovery.DefaultSchemaSpace);

            if (dbContext.Database.Connection.State == ConnectionState.Closed)
            {
                await dbContext.Database.Connection.OpenAsync();
            }

            return dbContext.Database.Connection;
        }

        public override void Dispose()
        {
        }
    }
}