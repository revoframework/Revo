using System;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Extensions.AutoMapper
{
    public class AutoMapperInitializer : IAutoMapperInitializer, IApplicationConfigurer
    {
        private readonly IAutoMapperProfileDiscovery profileDiscovery;
        private readonly Lazy<MapperConfiguration> mapperConfiguration;
        private readonly IServiceLocator serviceLocator;

        public AutoMapperInitializer(IAutoMapperProfileDiscovery profileDiscovery,
            IServiceLocator serviceLocator)
        {
            this.profileDiscovery = profileDiscovery;
            this.serviceLocator = serviceLocator;

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
            });

            return config;
        }
    }
}
