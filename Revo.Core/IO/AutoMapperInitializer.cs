using AutoMapper;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Core.IO
{
    public class AutoMapperInitializer : IApplicationConfigurer
    {
        private readonly IAutoMapperProfileDiscovery profileDiscovery;

        public AutoMapperInitializer(IAutoMapperProfileDiscovery profileDiscovery)
        {
            this.profileDiscovery = profileDiscovery;
        }

        public void Configure()
        {
            Mapper.Initialize(config =>
            {
                var profiles = profileDiscovery.DiscoverProfiles();
                foreach (Profile profile in profiles)
                {
                    config.AddProfile(profile);
                }
            });
        }
    }
}
