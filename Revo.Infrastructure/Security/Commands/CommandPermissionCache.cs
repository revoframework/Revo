using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Collections;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Core.Security;
using Revo.Core.Types;

namespace Revo.Infrastructure.Security.Commands
{
    public class CommandPermissionCache : ICommandPermissionCache, IApplicationStartedListener
    {
        private readonly IPermissionTypeRegistry permissionTypeRegistry;
        private readonly PermissionTypeIndexer permissionTypeIndexer;
        private readonly ITypeExplorer typeExplorer;
        private Lazy<Dictionary<Type, CommandTypeInfo>> commandTypePermissions;

        public CommandPermissionCache(IPermissionTypeRegistry permissionTypeRegistry,
            PermissionTypeIndexer permissionTypeIndexer,
            ITypeExplorer typeExplorer)
        {
            this.permissionTypeRegistry = permissionTypeRegistry;
            this.permissionTypeIndexer = permissionTypeIndexer;
            this.typeExplorer = typeExplorer;

            Clear();
        }

        public void OnApplicationStarted()
        {
            Clear();
        }

        public IReadOnlyCollection<Permission> GetCommandPermissions(ICommandBase command)
        {
            if (commandTypePermissions.Value.TryGetValue(command.GetType(), out var info))
            {
                return info.Permissions;
            }

            throw new ArgumentException(
                $"Unknown command type passed to GetCommandPermissions: {command.GetType().FullName}");
        }

        public bool IsAuthenticationRequired(ICommandBase command)
        {
            if (commandTypePermissions.Value.TryGetValue(command.GetType(), out var info))
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

                foreach (var commandType in typeExplorer
                    .GetAllTypes()
                    .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition)
                    .Where(x => typeof(ICommandBase).IsAssignableFrom(x)))
                {
                    var permissions = GetCommandTypePermissions(commandType).ToArray();
                    bool isAuthenticated = IsAuthenticationRequired(commandType);
                    dict.Add(commandType, new CommandTypeInfo(isAuthenticated, permissions));
                }

                return dict;
            });
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
