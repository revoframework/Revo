using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Core.Commands
{
    public class CommandHandlerDiscovery : IApplicationConfigurer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ITypeExplorer typeExplorer;
        private readonly StandardKernel kernel;

        public CommandHandlerDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel)
        {
            this.typeExplorer = typeExplorer;
            this.kernel = kernel;
        }

        public void Configure()
        {
            DiscoverCommandHandlers();
        }

        private void DiscoverCommandHandlers()
        {
            var commandHandlerTypes = typeExplorer.GetAllTypes()
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition
                    && CommandBindExtensions.GetCommandHandlerInterfaces(x).Length > 0)
                .ToArray();

            RegisterCommandHandlers(commandHandlerTypes);
            Logger.Debug($"Discovered {commandHandlerTypes.Length} command handlers: {string.Join(", ", commandHandlerTypes.Select(x => x.FullName))}");
        }

        private void RegisterCommandHandlers(IEnumerable<Type> commandHandlerTypes)
        {
            foreach (Type commandHandlerType in commandHandlerTypes)
            {
                CommandBindExtensions
                    .BindCommandHandler(kernel, commandHandlerType)
                    .InTaskScope();
            }
        }
    }
}
