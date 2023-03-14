using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ninject;
using Revo.Core.Types;

namespace Revo.EF6.DataAccess.Model
{
    public class ModelDefinitionDiscovery
    {
        private readonly ITypeExplorer typeExplorer;
        private readonly StandardKernel kernel;
        private readonly ILogger logger;

        public ModelDefinitionDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel, ILogger logger)
        {
            this.typeExplorer = typeExplorer;
            this.kernel = kernel;
            this.logger = logger;
        }
        
        public IEnumerable<IModelDefinition> DiscoverModelDefinitions()
        {
            var modelDefinitionTypes = typeExplorer.GetAllTypes()
                .Where(x => typeof(IModelDefinition).IsAssignableFrom(x)
                            && x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition);

            RegisterModelDefinitions(modelDefinitionTypes);

            List<IModelDefinition> modelDefinitions = GetModelDefinitions();
            logger.LogDebug($"Found {modelDefinitions.Count} EF6 model definitions: {string.Join(", ", modelDefinitions.Select(x => x.GetType().FullName))}");
            return modelDefinitions;
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
