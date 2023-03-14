using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Commands;
using Revo.Core.Lifecycle;
using Revo.Core.Security;

namespace Revo.Infrastructure.Security.Commands
{
    public class CommandPermissionCache : ICommandPermissionCache, IApplicationStartedListener
    {
        private readonly IPermissionTypeRegistry permissionTypeRegistry;
        private readonly IPermissionTypeIndexer permissionTypeIndexer;
        private readonly ICommandTypeDiscovery commandTypeDiscovery;
        private Lazy<Dictionary<Type, CommandTypeInfo>> commandTypePermissions;

        public CommandPermissionCache(IPermissionTypeRegistry permissionTypeRegistry,
            IPermissionTypeIndexer permissionTypeIndexer,
            ICommandTypeDiscovery commandTypeDiscovery)
        {
            this.permissionTypeRegistry = permissionTypeRegistry;
            this.permissionTypeIndexer = permissionTypeIndexer;
            this.commandTypeDiscovery = commandTypeDiscovery;

            Clear();
        }

        public void OnApplicationStarted()
        {
            Clear();
        }

        public IReadOnlyCollection<Permission> GetCommandPermissions(ICommandBase command)
        {
            var commandType = GetCommandType(command);
            if (commandTypePermissions.Value.TryGetValue(commandType, out var info))
            {
                return info.Permissions;
            }

            throw new ArgumentException(
                $"Unknown command type passed to GetCommandPermissions: {command.GetType().FullName}");
        }

        public bool IsAuthenticationRequired(ICommandBase command)
        {
            var commandType = GetCommandType(command);
            if (commandTypePermissions.Value.TryGetValue(commandType, out var info))
            {
                return info.IsAuthenticationRequired;
            }

            throw new ArgumentException(
                $"Unknown command type passed to IsAuthenticationRequired: {command.GetType().FullName}");
        }

        private void Clear()
        {
            commandTypePermissions = new Lazy<Dictionary<Type, CommandTypeInfo>>(() =>
            {
                var dict = new Dictionary<Type, CommandTypeInfo>();
                permissionTypeIndexer.EnsureIndexed();

                foreach (var commandType in commandTypeDiscovery.DiscoverCommandTypes())
                {
                    var permissions = GetCommandTypePermissions(commandType).ToArray();
                    bool isAuthenticated = IsAuthenticationRequired(commandType);
                    dict.Add(commandType, new CommandTypeInfo(isAuthenticated, permissions));
                }

                return dict;
            });
        }

        private Type GetCommandType(ICommandBase command)
        {
            var commandType = command.GetType();
            if (commandType.IsConstructedGenericType)
            {
                commandType = commandType.GetGenericTypeDefinition();
            }

            return commandType;
        }

        private IEnumerable<Permission> GetCommandTypePermissions(Type type)
        {
            IEnumerable<Permission> permissions = type
                .GetCustomAttributes(typeof(AuthorizePermissionsAttribute), true)
                .SelectMany(x => ((AuthorizePermissionsAttribute)x).PermissionIds)
                .Select(x => new Permission(permissionTypeRegistry.GetPermissionTypeById(x),
                    null, null));
            
            return permissions;
        }

        private bool IsAuthenticationRequired(Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(AuthenticatedAttribute), true);
            return attributes.Length > 0;
        }

        public class CommandTypeInfo
        {
            public CommandTypeInfo(bool isAuthenticationRequired, IReadOnlyCollection<Permission> permissions)
            {
                IsAuthenticationRequired = isAuthenticationRequired;
                Permissions = permissions;
            }

            public bool IsAuthenticationRequired { get; }
            public IReadOnlyCollection<Permission> Permissions { get; }
        }
    }
}
