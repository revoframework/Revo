using AutoMapper;
using Revo.Core.Core;
using Revo.Core.IO;
using Revo.Core.Lifecycle;

namespace Revo.Platforms.AspNet.Core
{
    public class AutoMapperInitializer : IApplicationStartListener
    {
        private readonly ITypeExplorer typeExplorer;
        private readonly AutoMapperDefinitionDiscovery autoMapperDefinitionDiscovery;

        public AutoMapperInitializer(ITypeExplorer typeExplorer,
            AutoMapperDefinitionDiscovery autoMapperDefinitionDiscovery)
        {
            this.typeExplorer = typeExplorer;
            this.autoMapperDefinitionDiscovery = autoMapperDefinitionDiscovery;
        }

        public void OnApplicationStarted()
        {
            Mapper.Initialize(config =>
            {
                var definitions = autoMapperDefinitionDiscovery.DiscoverAutoMapperDefinitions();
                foreach (IAutoMapperDefinition definition in definitions)
                {
                    definition.Configure(config);
                }
            });
        }
    }
}
