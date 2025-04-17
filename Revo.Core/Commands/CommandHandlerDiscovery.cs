﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ninject;
using Revo.Core.Collections;
using Revo.Core.Lifecycle;
using Revo.Core.Types;

namespace Revo.Core.Commands
{
    public class CommandHandlerDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel,
        ICommandRouter commandRouter, ILogger logger) : IApplicationConfigurer
    {
        private readonly ITypeExplorer typeExplorer = typeExplorer;
        private readonly StandardKernel kernel = kernel;
        private readonly ICommandRouter commandRouter = commandRouter;
        private readonly ILogger logger = logger;

        public void Configure() => DiscoverCommandHandlers();

        private void DiscoverCommandHandlers()
        {
            var commandHandlerTypes = typeExplorer.GetAllTypes()
                .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition);

            var handlersToInterfaces = new MultiValueDictionary<Type, Type>();
            foreach (var commandHandlerType in commandHandlerTypes)
            {
                var interfaces = CommandHandlerBindingExtensions.GetCommandHandlerInterfaces(commandHandlerType);
                if (interfaces.Length > 0)
                {
                    handlersToInterfaces.AddRange(commandHandlerType, interfaces);
                }
            }

            RegisterCommandHandlers(handlersToInterfaces);
            logger.LogDebug($"Discovered {handlersToInterfaces.Count} command handlers: {string.Join(", ", commandHandlerTypes.Select(x => x.FullName))}");
        }

        private void RegisterCommandHandlers(IReadOnlyDictionary<Type, IReadOnlyCollection<Type>> handlersToInterfaces)
        {
            Func<ICommandBus> commandBusLambda = () => kernel.Get<ILocalCommandBus>();

            foreach (var commandHandlerType in handlersToInterfaces)
            {
                kernel.BindCommandHandler(commandHandlerType.Key);

                foreach (var handlerInterface in commandHandlerType.Value)
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
