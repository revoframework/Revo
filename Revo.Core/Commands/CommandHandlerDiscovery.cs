using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Revo.Core.Collections;
using Revo.Core.Commands.Configuration;
using Revo.Core.Configuration;
using Revo.Core.Types;

namespace Revo.Core.Commands;

public class CommandHandlerDiscovery(IRevoConfiguration configuration)
{
    public void DiscoverCommandHandlers(IServiceCollection serviceCollection)
    {
        var typeExplorer = new TypeExplorer(
            configuration.GetSection<CoreConfigurationSection>(),
            NullLogger<TypeExplorer>.Instance);
            
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

        RegisterCommandHandlers(handlersToInterfaces, serviceCollection);
        //logger.LogDebug($"Discovered {handlersToInterfaces.Count} command handlers: {string.Join(", ", commandHandlerTypes.Select(x => x.FullName))}");
    }

    private void RegisterCommandHandlers(IReadOnlyDictionary<Type, IReadOnlyCollection<Type>> handlersToInterfaces,
        IServiceCollection serviceCollection)
    {
        var commandsConfig = configuration.GetSection<CommandsConfigurationSection>();
            
        Func<IServiceProvider, ICommandBus> commandBusLambda = (serviceProvider)
            => serviceProvider.GetRequiredService<ILocalCommandBus>();

        foreach (var commandHandlerType in handlersToInterfaces)
        {
            if (!serviceCollection.Any(x => x.ServiceType == commandHandlerType.Key))
            {
                serviceCollection.AddCommandHandler(commandHandlerType.Key);
            }
                
            foreach (var handlerInterface in commandHandlerType.Value)
            {
                var commandType = handlerInterface.GetGenericArguments()[0];
                if (!commandsConfig.CommandRoutes.ContainsKey(commandType))
                {
                    commandsConfig.AddCommandRoute(commandBusLambda, commandType);
                }
            }
        }
    }
}