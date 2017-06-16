using System;
using System.Collections.Generic;
using System.Linq;
using GTRevo.Core;
using Ninject;

namespace GTRevo.DataAccess.EF6.Model
{
    public class ModelDefinitionDiscovery
    {
        private readonly ITypeExplorer typeExplorer;
        private readonly StandardKernel kernel;

        public ModelDefinitionDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel)
        {
            this.typeExplorer = typeExplorer;
            this.kernel = kernel;
        }
        
        public IEnumerable<IModelDefinition> DiscoverModelDefinitions()
        {
            var modelDefinitionTypes = typeExplorer.GetAllTypes()
                .Where(x => typeof(IModelDefinition).IsAssignableFrom(x)
                            && x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition);

            RegisterModelDefinitions(modelDefinitionTypes);
            return GetModelDefinitions();
        }

        private void RegisterModelDefinitions(IEnumerable<Type> modelDefinitionTypes)
        {
            var availableModelDefinitions = GetModelDefinitions();

            foreach (Type modelDefinitionType in modelDefinitionTypes)
            {
                if (!availableModelDefinitions.Any(x => x.GetType() == modelDefinitionType))
                {
                    kernel.Bind<IModelDefinition>()
                        .To(modelDefinitionType)
                        .InSingletonScope();
                }
            }
        }

        private List<IModelDefinition> GetModelDefinitions()
        {
            return kernel.GetAll<IModelDefinition>().ToList();
        }
    }
}
