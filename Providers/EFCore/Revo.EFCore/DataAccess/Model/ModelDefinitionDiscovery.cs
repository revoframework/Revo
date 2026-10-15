using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Revo.Core.Types;

namespace Revo.EFCore.DataAccess.Model
{
    public class ModelDefinitionDiscovery
    {
        private readonly ITypeExplorer typeExplorer;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ModelDefinitionDiscovery> logger;

        public ModelDefinitionDiscovery(ITypeExplorer typeExplorer, IServiceProvider serviceProvider,
            ILogger<ModelDefinitionDiscovery> logger)
        {
            this.typeExplorer = typeExplorer;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }
        
        public IEnumerable<IEFCoreModelDefinition> DiscoverModelDefinitions()
        {
            List<IEFCoreModelDefinition> modelDefinitions = serviceProvider.GetServices<IEFCoreModelDefinition>().ToList();
            logger.LogDebug($"Found {modelDefinitions.Count} EF Core model definitions: {string.Join(", ", modelDefinitions.Select(x => x.GetType().FullName))}");
            return modelDefinitions;
        }
    }
}
