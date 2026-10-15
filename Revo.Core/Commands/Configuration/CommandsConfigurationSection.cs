using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Configuration;

namespace Revo.Core.Commands.Configuration;

public class CommandsConfigurationSection : IRevoConfigurationSection
{
    private readonly Dictionary<Type, Func<IServiceProvider, ICommandBus>> commandRoutes = new();
    private readonly IRevoConfiguration revoConfiguration;

    public CommandsConfigurationSection(IRevoConfiguration revoConfiguration)
    {
        this.revoConfiguration = revoConfiguration;
    }

    public bool AutoDiscoverCommandHandlers { get; set; } = true;
    public IReadOnlyDictionary<Type, Func<IServiceProvider, ICommandBus>> CommandRoutes => commandRoutes;

    public CommandsConfigurationSection AddCommandRoute<TCommand>(Func<IServiceProvider, ICommandBus> commandBusFunc) where TCommand : ICommandBase
    {
        return AddCommandRoute(commandBusFunc, typeof(TCommand));
    }

    public CommandsConfigurationSection AddCommandRoute(Func<IServiceProvider, ICommandBus> commandBusFunc, params Type[] commandTypes)
    {
        var duplicates = commandTypes.Where(commandRoutes.ContainsKey).ToArray();
        if (duplicates.Length > 0)
        {
            throw new ConfigurationException($"Specified command type(s) already have a route to a command bus: {string.Join(", ", duplicates.Select(x => x.FullName))}");
        }

        var nonCommands = commandTypes.Where(x => !typeof(ICommandBase).IsAssignableFrom(x)).ToArray();
        if (nonCommands.Length > 0)
        {
            throw new ConfigurationException($"Specified types are not command: {string.Join(", ", nonCommands.Select(x => x.FullName))}");
        }

        foreach (Type commandType in commandTypes)
        {
            commandRoutes[commandType] = commandBusFunc;
        }

        return this;
    }
        
    public void AddCommandHandler<TCommandHandler, TCommand>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TCommandHandler : class, ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        AddCommandHandler(typeof(TCommandHandler), serviceLifetime, typeof(TCommand));
    }
        
    public void AddCommandHandler<TCommandHandler>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TCommandHandler : class
    {
        var commandHandlerTypes = CommandHandlerBindingExtensions
            .GetCommandHandlerInterfaces(typeof(TCommandHandler));
            
        var commandTypes = commandHandlerTypes
            .Select(x => x.GetGenericArguments()[0])
            .ToArray();
            
        AddCommandHandler(typeof(TCommandHandler), serviceLifetime, commandTypes);
    }
        
    public void AddCommandHandler(Type commandHandlerType, ServiceLifetime serviceLifetime, params Type[] commandTypes)
    {
        var nonCommands = commandTypes.Where(x => !typeof(ICommandBase).IsAssignableFrom(x)).ToArray();
        if (nonCommands.Length > 0)
        {
            throw new ConfigurationException($"Specified types are not command: {string.Join(", ", nonCommands.Select(x => x.FullName))}");
        }
            
        revoConfiguration.ConfigureServices(
            serviceCollection => serviceCollection.AddCommandHandler(commandHandlerType));

        foreach (Type commandType in commandTypes)
        {
            AddCommandRoute(serviceProvider => serviceProvider.GetRequiredService<ILocalCommandBus>(),
                commandType);
        }
    }
}