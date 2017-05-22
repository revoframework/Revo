using System.Reflection;
using AutoMapper;
using AutoMapper.Attributes;
using GTRevo.Platform.Core.Lifecycle;

namespace GTRevo.Platform.Core
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
                foreach (Assembly assembly in typeExplorer.GetAllReferencedAssemblies())
                {
                    assembly.MapTypes(config);
                }

                var definitions = autoMapperDefinitionDiscovery.DiscoverAutoMapperDefinitions();
                foreach (IAutoMapperDefinition definition in definitions)
                {
                    definition.Configure(config);
                }
            });
        }
    }
}
