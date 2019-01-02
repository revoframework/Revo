using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Revo.Core.Lifecycle;

namespace Revo.Core.IO
{
    public class AutoMapperInitializer : IApplicationConfigurer
    {
        private readonly IAutoMapperProfileDiscovery profileDiscovery;

        public AutoMapperInitializer(IAutoMapperProfileDiscovery profileDiscovery)
        {
            this.profileDiscovery = profileDiscovery;
        }

        public bool AutoDiscoverAutoMapperProfiles { get; set; } = true;

        public void Configure()
        {
            Mapper.Initialize(config =>
            {
                config.AddExpressionMapping();

                if (AutoDiscoverAutoMapperProfiles)
                {
                    profileDiscovery.DiscoverProfiles();
                }

                var profiles = profileDiscovery.GetProfiles();
                foreach (Profile profile in profiles)
                {
                    config.AddProfile(profile);
                }
            });
        }
    }
}
