using System;
using Revo.Core.Configuration;
using Revo.RavenDB.Projections;

namespace Revo.RavenDB
{
    public static class RavenConfigurationExtensions
    {
        public static IRevoConfiguration UseRavenDataAccess(this IRevoConfiguration configuration,
            bool? useAsPrimaryRepository = true,
            RavenConnectionConfiguration connection = null,
            Action<RavenConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<RavenConfigurationSection>();
            section.IsActive = true;
            section.Connection = connection ?? section.Connection;
            section.UseAsPrimaryRepository = useAsPrimaryRepository ?? section.UseAsPrimaryRepository;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    c.LoadModule(new RavenModule(section.Connection, section.UseAsPrimaryRepository));
                }
            });

            return configuration;
        }

        public static IRevoConfiguration UseRavenProjections(this IRevoConfiguration configuration,
            Action<RavenConfigurationSection> advancedAction = null)
        {
            configuration.UseRavenDataAccess(null);

            var section = configuration.GetSection<RavenConfigurationSection>();
            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseProjections)
                {
                    c.LoadModule(new RavenProjectionsModule());
                }
            });

            return configuration;
        }
    }
}
