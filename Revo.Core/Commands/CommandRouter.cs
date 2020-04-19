using System;
using System.Collections.Generic;
using NLog;

namespace Revo.Core.Commands
{
    public class CommandRouter : ICommandRouter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<Type, Func<ICommandBus>> routes = new Dictionary<Type, Func<ICommandBus>>();

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
            Logger.Info($"Registered route for command type {commandType}");
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

            Logger.Info($"Unregistered route for command type {commandType}");
        }
    }
}