using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using Revo.Core.Types;

namespace Revo.EFCore.DataAccess.Model
{
    public class ModelDefinitionDiscovery
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ITypeExplorer typeExplorer;
        private readonly StandardKernel kernel;

        public ModelDefinitionDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel)
        {
            this.typeExplorer = typeExplorer;
            this.kernel = kernel;
        }
        
        public IEnumerable<IEFCoreModelDefinition> DiscoverModelDefinitions()
        {
            var modelDefinitionTypes = typeExplorer.GetAllTypes()
                .Where(x => typeof(IEFCoreModelDefinition).IsAssignableFrom(x)
                            && x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition);

            RegisterModelDefinitions(modelDefinitionTypes);

            List<IEFCoreModelDefinition> modelDefinitions = GetModelDefinitions();
            Logger.Debug($"Found {modelDefinitions.Count} EF Core model definitions: {string.Join(", ", modelDefinitions.Select(x => x.GetType().FullName))}");
            return modelDefinitions;
        }

        private void RegisterModelDefinitions(IEnumerable<Type> modelDefinitionTypes)
        {
            var availableModelDefinitions = GetModelDefinitions();

            foreach (Type modelDefinitionType in modelDefinitionTypes)
            {
                if (!availableModelDefinitions.Any(x => x.GetType() == modelDefinitionType))
                {
                    kernel.Bind<IEFCoreModelDefinition>()
                        .To(modelDefinitionType)
                        .InSingletonScope();
                }
            }
        }

        private List<IEFCoreModelDefinition> GetModelDefinitions()
        {
            return kernel.GetAll<IEFCoreModelDefinition>().ToList();
        }
    }
}
