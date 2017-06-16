using System;
using System.Collections.Generic;
using System.Linq;
using GTRevo.Core;
using Ninject;

namespace GTRevo.Platform.Core
{
    public class AutoMapperDefinitionDiscovery
    {
        private readonly ITypeExplorer typeExplorer;
        private readonly StandardKernel kernel;

        public AutoMapperDefinitionDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel)
        {
            this.typeExplorer = typeExplorer;
            this.kernel = kernel;
        }

        public IEnumerable<IAutoMapperDefinition> DiscoverAutoMapperDefinitions()
        {
            var modelDefinitionTypes = typeExplorer.GetAllTypes()
                .Where(x => typeof(IAutoMapperDefinition).IsAssignableFrom(x)
                            && x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition);

            RegisterDefinitions(modelDefinitionTypes);
            return GetDefinitions();
        }

        private void RegisterDefinitions(IEnumerable<Type> modelDefinitionTypes)
        {
            var availableModelDefinitions = GetDefinitions();

            foreach (Type modelDefinitionType in modelDefinitionTypes)
            {
                if (!availableModelDefinitions.Any(x => x.GetType() == modelDefinitionType))
                {
                    kernel.Bind<IAutoMapperDefinition>()
                        .To(modelDefinitionType)
                        .InSingletonScope();
                }
            }
        }

        private List<IAutoMapperDefinition> GetDefinitions()
        {
            return kernel.GetAll<IAutoMapperDefinition>().ToList();
        }
    }
}
