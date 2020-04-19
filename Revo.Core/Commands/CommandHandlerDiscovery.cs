using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Core.Commands
{
    public class CommandHandlerDiscovery : IApplicationConfigurer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ITypeExplorer typeExplorer;
        private readonly StandardKernel kernel;
        private readonly ICommandRouter commandRouter;

        public CommandHandlerDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel,
            ICommandRouter commandRouter)
        {
            this.typeExplorer = typeExplorer;
            this.kernel = kernel;
            this.commandRouter = commandRouter;
        }

        public void Configure()
        {
            DiscoverCommandHandlers();
        }

        private void DiscoverCommandHandlers()
        {
            var commandHandlerTypes = typeExplorer.GetAllTypes()
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition
                    && CommandHandlerRegistrationExtensions.GetCommandHandlerInterfaces(x).Length > 0)
                .ToArray();

            RegisterCommandHandlers(commandHandlerTypes);
            Logger.Debug($"Discovered {commandHandlerTypes.Length} command handlers: {string.Join(", ", commandHandlerTypes.Select(x => x.FullName))}");
        }

        private void RegisterCommandHandlers(IEnumerable<Type> commandHandlerTypes)
        {
            Func<ICommandBus> commandBusLambda = () => kernel.Get<ILocalCommandBus>();

            foreach (Type commandHandlerType in commandHandlerTypes)
            {
                CommandHandlerRegistrationExtensions
                    .BindLocalCommandHandle(kernel, commandHandlerType);

                var interfaces = CommandHandlerRegistrationExtensions
                    .GetCommandHandlerInterfaces(commandHandlerType);

                foreach (var handlerInterface in interfaces)
                {
                    var commandType = handlerInterface.GetGenericArguments()[0];
                    if (!commandRouter.HasRoute(commandType))
                    {
                        commandRouter.AddRoute(commandType, commandBusLambda);
                    }
                }
            }
        }
    }
}
