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
    public class CommandPermissionCache : ICommandPermissionCache, IApplicationStartListener
    {
        private readonly IPermissionTypeRegistry permissionTypeRegistry;
        private readonly PermissionTypeIndexer permissionTypeIndexer;
        private readonly ITypeExplorer typeExplorer;
        private Lazy<MultiValueDictionary<Type, Permission>> commandTypePermissions;

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
            if (commandTypePermissions.Value.TryGetValue(command.GetType(), out var permissions))
            {
                return permissions;
            }

            throw new ArgumentException(
                $"Unknown command type to GetCommandPermissions for: {command.GetType().FullName}");
        }

        private void Clear()
        {
            commandTypePermissions = new Lazy<MultiValueDictionary<Type, Permission>>(() =>
            {
                var dict = new MultiValueDictionary<Type, Permission>();
                permissionTypeIndexer.EnsureIndexed();

                foreach (var commandType in typeExplorer
                    .GetAllTypes()
                    .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition)
                    .Where(x => typeof(ICommandBase).IsAssignableFrom(x)))
                {
                    dict.AddRange(commandType, GetCommandTypePermissions(commandType));
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
    }
}
