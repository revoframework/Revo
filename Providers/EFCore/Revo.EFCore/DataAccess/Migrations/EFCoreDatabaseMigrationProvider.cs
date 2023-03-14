using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Revo.Core.Events;
using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.EFCore.DataAccess.Migrations
{
    public class EFCoreDatabaseMigrationProvider : AdoNetStubDatabaseMigrationProvider
    {
        private readonly IEFCoreDatabaseAccess databaseAccess;

        public EFCoreDatabaseMigrationProvider(IEFCoreDatabaseAccess databaseAccess,
            IMigrationScripterFactory scripterFactory,
            IEventBus eventBus, ILogger logger) : base(eventBus, logger)
        {
            this.databaseAccess = databaseAccess;
            
            Scripter = scripterFactory.GetProviderScripter(databaseAccess);
        }

        protected override IDatabaseMigrationScripter Scripter { get; }

        protected override async Task<IDbTransaction> BeginDbTransactionAsync(IDbConnection dbConnection)
        {
            var dbContext = databaseAccess.GetDbContext(EFCoreDatabaseAccess.DefaultSchemaSpace);
            var dbContextTransaction = await dbContext.Database.BeginTransactionAsync();
            return dbContextTransaction.GetDbTransaction();
        }

        protected override async Task CommitDbTransactionAsync(IDbTransaction dbTransaction)
        {
            var dbContext = databaseAccess.GetDbContext(EFCoreDatabaseAccess.DefaultSchemaSpace);
            if (dbContext.Database.CurrentTransaction == null
                || dbContext.Database.CurrentTransaction.GetDbTransaction() != dbTransaction)
            {
                throw new ArgumentException("Invalid dbTransaction passed");
            }
            
            await dbContext.Database.CurrentTransaction.CommitAsync();
        }

        protected override async Task RollbackDbTransactionAsync(IDbTransaction dbTransaction)
        {
            var dbContext = databaseAccess.GetDbContext(EFCoreDatabaseAccess.DefaultSchemaSpace);
            if (dbContext.Database.CurrentTransaction == null
                || dbContext.Database.CurrentTransaction.GetDbTransaction() != dbTransaction)
            {
                throw new ArgumentException("Invalid dbTransaction passed");
            }
            
            await dbContext.Database.CurrentTransaction.RollbackAsync();
        }

        public override string[] GetProviderEnvironmentTags()
        {
            List<string> tags = new List<string>(base.GetProviderEnvironmentTags());
            tags.Add("efcore");

            return tags.ToArray();
        }

        protected override async Task<IDbConnection> GetDbConnectionAsync()
        {
            var dbContext = databaseAccess.GetDbContext(EFCoreDatabaseAccess.DefaultSchemaSpace);
            
            if (dbContext.Database.GetDbConnection().State == ConnectionState.Closed)
            {
                await dbContext.Database.OpenConnectionAsync();
            }

            return dbContext.Database.GetDbConnection();
        }

        public override void Dispose()
        {
        }
    }
}