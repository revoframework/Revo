using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.EF6.DataAccess.Entities;
using Revo.EF6.DataAccess.Model;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.EF6.DataAccess.Migrations
{
    public class EF6DatabaseMigrationProvider : AdoNetStubDatabaseMigrationProvider
    {
        private readonly EF6ConnectionConfiguration connectionConfiguration;
        private readonly IEF6DatabaseAccess databaseAccess;

        public EF6DatabaseMigrationProvider(EF6ConnectionConfiguration connectionConfiguration, IMigrationScripterFactory scripterFactory,
            IEventBus eventBus, IEF6DatabaseAccess databaseAccess) : base(eventBus)
        {
            this.connectionConfiguration = connectionConfiguration;
            this.databaseAccess = databaseAccess;

            Scripter = scripterFactory.GetProviderScripter(connectionConfiguration);
        }

        protected override IDatabaseMigrationScripter Scripter { get; }

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