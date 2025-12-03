using System;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.Extensions.Logging;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Extensions.AutoMapper.Configuration;

namespace Revo.Extensions.AutoMapper
{
    public class AutoMapperInitializer : IAutoMapperInitializer, IApplicationConfigurer
    {
        private readonly IAutoMapperProfileDiscovery profileDiscovery;
        private readonly Lazy<MapperConfiguration> mapperConfiguration;
        private readonly IServiceLocator serviceLocator;
        private readonly AutoMapperConfigurationSection configurationSection;

        public AutoMapperInitializer(IAutoMapperProfileDiscovery profileDiscovery,
            IServiceLocator serviceLocator, AutoMapperConfigurationSection configurationSection)
        {
            this.profileDiscovery = profileDiscovery;
            this.serviceLocator = serviceLocator;
            this.configurationSection = configurationSection;

            mapperConfiguration = new Lazy<MapperConfiguration>(CreateMapperConfiguration);
        }

        public void Configure()
        {
            GetMapperConfiguration();
        }

        public MapperConfiguration GetMapperConfiguration()
        {
            return mapperConfiguration.Value;
        }

        public IMapper CreateMapper()
        {
            return GetMapperConfiguration().CreateMapper();
        }

        private MapperConfiguration CreateMapperConfiguration()
        {
            profileDiscovery.DiscoverProfiles();
            profileDiscovery.GetProfiles();

            var config = new MapperConfiguration(configExpression =>
            {
                configExpression.AddExpressionMapping();
                configExpression.ConstructServicesUsing(type => serviceLocator.Get(type));

                var profiles = profileDiscovery.GetProfiles();
                foreach (Profile profile in profiles)
                {
                    configExpression.AddProfile(profile);
                }
                
                configurationSection.ConfigureAction?.Invoke(configExpression);
            });

            return config;
        }
    }
}
