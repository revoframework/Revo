using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Revo.Core.Commands.Configuration;

namespace Revo.Core.Commands;

public class CommandRouter : ICommandRouter
{
    private readonly ILogger<CommandRouter> logger;
    private readonly Dictionary<Type, Func<IServiceProvider, ICommandBus>> routes = new();

    public CommandRouter(ILogger<CommandRouter> logger, CommandsConfigurationSection configurationSection)
    {
        this.logger = logger;
            
        foreach (var route in configurationSection.CommandRoutes)
        {
            AddRoute(route.Key, route.Value);
        }
    }

    public void AddRoute(Type commandType, Func<IServiceProvider, ICommandBus> commandBus)
    {
        if (!typeof(ICommandBase).IsAssignableFrom(commandType))
        {
            throw new ArgumentException($"Specified type is not a command: {commandType}");
        }

        if (routes.TryGetValue(commandType, out var existingBus))
        {
            throw new ArgumentException($"Command type {commandType} is already bound to {existingBus}");
        }

        routes[commandType] = commandBus;
        logger.LogInformation($"Registered route for command type {commandType}");
    }

    public Func<IServiceProvider, ICommandBus> FindRoute(Type commandType)
    {
        if (!typeof(ICommandBase).IsAssignableFrom(commandType))
        {
            throw new ArgumentException($"Specified type is not a command: {commandType}");
        }

        routes.TryGetValue(commandType, out var route);
        return route;
    }

    public bool HasRoute(Type commandType)
    {
        if (!typeof(ICommandBase).IsAssignableFrom(commandType))
        {
            throw new ArgumentException($"Specified type is not a command: {commandType}");
        }

        return routes.ContainsKey(commandType);
    }

    public void RemoveRoute(Type commandType)
    {
        if (!typeof(ICommandBase).IsAssignableFrom(commandType))
        {
            throw new ArgumentException($"Specified type is not a command: {commandType}");
        }

        if (!routes.Remove(commandType))
        {
            throw new ArgumentException($"Command type {commandType} has no existing route");
        }

        logger.LogInformation($"Unregistered route for command type {commandType}");
    }
}