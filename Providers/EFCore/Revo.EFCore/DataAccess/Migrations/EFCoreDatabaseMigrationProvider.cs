using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            IEventBus eventBus) : base(eventBus)
        {
            this.databaseAccess = databaseAccess;
            
            Scripter = scripterFactory.GetProviderScripter(databaseAccess);
        }

        protected override IDatabaseMigrationScripter Scripter { get; }

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