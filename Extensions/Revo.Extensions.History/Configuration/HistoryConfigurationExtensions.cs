using System;
using Revo.Core.Configuration;
using Revo.Extensions.History.ChangeTracking;
using Revo.Infrastructure;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.Extensions.History.Configuration
{
    public static class NotificationsConfigurationExtensions
    {
        public static IRevoConfiguration AddHistoryExtension(this IRevoConfiguration configuration,
            bool? isChangeTrackingActive = true,
            Action<HistoryConfigurationSection> advancedAction = null)
        {
            configuration.ConfigureInfrastructure(config =>
            {
                config.DatabaseMigrations.AddScannedAssembly(new ResourceDatabaseMigrationDiscoveryAssembly(
                    typeof(HistoryModule).Assembly.FullName, "Sql"));
            });

            var section = configuration.GetSection<HistoryConfigurationSection>();
            section.IsChangeTrackingActive = isChangeTrackingActive ?? section.IsChangeTrackingActive;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsChangeTrackingActive)
                {
                    c.LoadModule(new HistoryModule());
                }
            });

            return configuration;
        }
    }
}