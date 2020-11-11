using System;
using Revo.Core.Configuration;
using Revo.RavenDB.Projections;

namespace Revo.RavenDB.Configuration
{
    public static class RavenConfigurationExtensions
    {
        public static IRevoConfiguration UseRavenDataAccess(this IRevoConfiguration configuration,
            RavenConnectionConfiguration connection,
            bool? useAsPrimaryRepository = true,
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
            bool autoDiscoverProjectors = true,
            Action<RavenConfigurationSection> advancedAction = null)
        {
            configuration.UseRavenDataAccess(null);

            var section = configuration.GetSection<RavenConfigurationSection>();
            section.UseProjections = true;
            section.AutoDiscoverProjectors = autoDiscoverProjectors;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.UseProjections)
                {
                    c.LoadModule(new RavenProjectionsModule(section));
                }
            });

            return configuration;
        }
    }
}
