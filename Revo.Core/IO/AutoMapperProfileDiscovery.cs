using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ninject;
using Revo.Core.Types;

namespace Revo.Core.IO
{
    public class AutoMapperProfileDiscovery : IAutoMapperProfileDiscovery
    {
        private readonly ITypeExplorer typeExplorer;
        private readonly StandardKernel kernel;

        public AutoMapperProfileDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel)
        {
            this.typeExplorer = typeExplorer;
            this.kernel = kernel;
        }

        public IReadOnlyCollection<Profile> DiscoverProfiles()
        {
            var modelDefinitionTypes = typeExplorer.GetAllTypes()
                .Where(x => typeof(Profile).IsAssignableFrom(x)
                            && x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition
                            && (x.AssemblyQualifiedName == null || !x.AssemblyQualifiedName.StartsWith("AutoMapper.")));

            RegisterProfiles(modelDefinitionTypes);
            return GetProfiles();
        }

        private void RegisterProfiles(IEnumerable<Type> profileTypes)
        {
            var availableProfiles = GetProfiles();

            foreach (Type profileType in profileTypes)
            {
                if (!availableProfiles.Any(x => x.GetType() == profileType))
                {
                    kernel.Bind<Profile>()
                        .To(profileType)
                        .InSingletonScope();
                }
            }
        }

        private List<Profile> GetProfiles()
        {
            return kernel.GetBindings(typeof(Profile)).Any()
                ? kernel.GetAll<Profile>().ToList()
                : new List<Profile>();
        }
    }
}
