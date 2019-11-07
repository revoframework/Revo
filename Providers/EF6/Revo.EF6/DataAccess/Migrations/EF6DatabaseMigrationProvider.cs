using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.EF6.DataAccess.Migrations
{
    public class EF6DatabaseMigrationProvider : AdoNetStubDatabaseMigrationProvider
    {
        private readonly EF6ConnectionConfiguration connectionConfiguration;
        private IDbConnection dbConnection;

        public EF6DatabaseMigrationProvider(EF6ConnectionConfiguration connectionConfiguration, IMigrationScripterFactory scripterFactory)
        {
            this.connectionConfiguration = connectionConfiguration;

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
            if (dbConnection == null)
            {
                dbConnection = connectionConfiguration.ConnectionFactory
                    .CreateConnection(connectionConfiguration.NameOrConnectionString);
            }

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            return dbConnection;
        }

        public override void Dispose()
        {
            dbConnection?.Dispose();
        }
    }
}