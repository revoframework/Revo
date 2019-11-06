using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoreLinq.Extensions;
using Revo.EFCore.DataAccess.Configuration;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.EFCore.DataAccess.Migrations
{
    public class EFCoreDatabaseMigrationProvider : AdoNetStubDatabaseMigrationProvider
    {

        private readonly IEFCoreConfigurer[] configurers;
        private DatabaseMigrationDbContext dbContext;

        public EFCoreDatabaseMigrationProvider(IEFCoreConfigurer[] configurers, IMigrationScripterFactory scripterFactory)
        {
            this.configurers = configurers;

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseMigrationDbContext>();
            configurers.ForEach(x => x.OnConfiguring(optionsBuilder));
            var options = optionsBuilder.Options;
            Scripter = scripterFactory.GetProviderScripter(options);
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
            if (dbContext == null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<DatabaseMigrationDbContext>();
                configurers.ForEach(x => x.OnConfiguring(optionsBuilder));
                dbContext = new DatabaseMigrationDbContext(optionsBuilder.Options);
                await dbContext.Database.EnsureCreatedAsync();
            }

            if (dbContext.Database.GetDbConnection().State == ConnectionState.Closed)
            {
                await dbContext.Database.OpenConnectionAsync();
            }

            return dbContext.Database.GetDbConnection();
        }

        public override void Dispose()
        {
            dbContext?.Dispose();
        }
    }
}