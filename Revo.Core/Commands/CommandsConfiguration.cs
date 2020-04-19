using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Configuration;

namespace Revo.Core.Commands
{
    public class CommandsConfiguration
    {
        private readonly Dictionary<Type, Func<ICommandBus>> commandBusRoutes =
            new Dictionary<Type, Func<ICommandBus>>();

        public bool AutoDiscoverCommandHandlers { get; set; } = true;
        public IReadOnlyDictionary<Type, Func<ICommandBus>> CommandRoutes => commandBusRoutes;

        public CommandsConfiguration AddCommandRoute<TCommand>(Func<ICommandBus> commandBusFunc) where TCommand : ICommandBase
        {
            return AddCommandRoute(commandBusFunc, typeof(TCommand));
        }

        public CommandsConfiguration AddCommandRoute(Func<ICommandBus> commandBusFunc, params Type[] commandTypes)
        {
            var duplicates = commandTypes.Where(commandBusRoutes.ContainsKey).ToArray();
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
                commandBusRoutes[commandType] = commandBusFunc;
            }

            return this;
        }
    }
}