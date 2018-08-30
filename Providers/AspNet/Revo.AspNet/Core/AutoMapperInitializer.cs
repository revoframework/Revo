using AutoMapper;
using Revo.Core.IO;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.AspNet.Core
{
    public class AutoMapperInitializer : IApplicationConfigurer
    {
        private readonly ITypeExplorer typeExplorer;
        private readonly AutoMapperDefinitionDiscovery autoMapperDefinitionDiscovery;

        public AutoMapperInitializer(ITypeExplorer typeExplorer,
            AutoMapperDefinitionDiscovery autoMapperDefinitionDiscovery)
        {
            this.typeExplorer = typeExplorer;
            this.autoMapperDefinitionDiscovery = autoMapperDefinitionDiscovery;
        }

        public void Configure()
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
