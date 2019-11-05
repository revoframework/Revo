using System;
using Revo.Core.Configuration;
using Revo.Infrastructure;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.Extensions.Notifications.Configuration
{
    public static class NotificationsConfigurationExtensions
    {
        public static IRevoConfiguration AddNotificationsExtension(this IRevoConfiguration configuration,
            Action<NotificationsConfigurationSection> advancedAction = null)
        {
            configuration.ConfigureInfrastructure(config =>
            {
                config.DatabaseMigrations.AddScannedAssembly(new ResourceDatabaseMigrationDiscoveryAssembly(
                    typeof(NotificationsModule).Assembly.FullName, "Sql"));
            });

            var section = configuration.GetSection<NotificationsConfigurationSection>();
            section.IsActive = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    c.LoadModule(new NotificationsModule());
                }
            });

            return configuration;
        }
    }
}