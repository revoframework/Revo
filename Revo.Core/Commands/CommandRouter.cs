using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Revo.Core.Commands
{
    public class CommandRouter : ICommandRouter
    {
        private readonly ILogger logger;

        private readonly Dictionary<Type, Func<ICommandBus>> routes = new Dictionary<Type, Func<ICommandBus>>();

        public CommandRouter(ILogger logger)
        {
            this.logger = logger;
        }

        public void AddRoute(Type commandType, Func<ICommandBus> commandBus)
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

        public ICommandBus FindRoute(Type commandType)
        {
            if (!typeof(ICommandBase).IsAssignableFrom(commandType))
            {
                throw new ArgumentException($"Specified type is not a command: {commandType}");
            }

            routes.TryGetValue(commandType, out var commandBus);
            return commandBus?.Invoke();
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
}