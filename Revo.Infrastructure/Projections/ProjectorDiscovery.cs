using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Infrastructure.Projections
{
    public class ProjectorDiscovery : IApplicationConfigurer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Type[] genericProjectorInterfaces;
        private readonly ITypeExplorer typeExplorer;
        private readonly StandardKernel kernel;

        public ProjectorDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel, Type[] genericProjectorInterfaces)
        {
            this.typeExplorer = typeExplorer;
            this.kernel = kernel;
            this.genericProjectorInterfaces = genericProjectorInterfaces;
        }

        public void Configure()
        {
            Discover();
        }

        protected virtual Type[] GetGenericProjectorInterfaces()
        {
            return genericProjectorInterfaces;
        }

        private void Discover()
        {
            var projectorTypes = typeExplorer.GetAllTypes()
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition
                    && GetProjectorInterfaces(x).Length > 0)
                .ToArray();

            RegisterProjectors(projectorTypes);
            Logger.Debug($"Discovered {projectorTypes.Length} projectors: {string.Join(", ", projectorTypes.Select(x => x.FullName))}");
        }
        
        private Type[] GetProjectorInterfaces(Type requestHandlerType)
        {
            var intfs = GetInterfaces(requestHandlerType);

            intfs = intfs
                .Where(x => x.IsGenericType
                            && GetGenericProjectorInterfaces()
                                .Contains(x.GetGenericTypeDefinition()));

            return intfs.Distinct().ToArray();
        }

        private static IEnumerable<Type> GetInterfaces(Type handlerType)
        {
            var intfs = (IEnumerable<Type>)handlerType.GetInterfaces();
            var nestedIntfs = intfs.SelectMany(x => x.GetInterfaces().Length > 0 ? GetInterfaces(x) : new Type[] { }).ToArray();
            if (nestedIntfs.Length > 0)
            {
                intfs = intfs.Concat(nestedIntfs);
            }

            return intfs;
        }

        private void RegisterProjectors(IEnumerable<Type> projectorTypes)
        {
            foreach (Type projectorType in projectorTypes)
            {
                kernel
                    .Bind(GetProjectorInterfaces(projectorType))
                    .To(projectorType)
                    .InTaskScope();
            }
        }
    }
}
