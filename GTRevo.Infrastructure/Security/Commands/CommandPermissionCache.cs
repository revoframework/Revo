using System;
using System.Collections.Generic;
using System.Linq;
using GTRevo.Core.Commands;
using GTRevo.Core.Core;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.Platform.Security;

namespace GTRevo.Infrastructure.Security.Commands
{
    public class CommandPermissionCache : IApplicationStartListener
    {
        private readonly IPermissionTypeRegistry permissionTypeRegistry;
        private readonly PermissionTypeIndexer permissionTypeIndexer;
        private readonly ITypeExplorer typeExplorer;
        private readonly Dictionary<Type, List<Permission>> commandTypePermissions = new Dictionary<Type, List<Permission>>();

        public CommandPermissionCache(IPermissionTypeRegistry permissionTypeRegistry,
            PermissionTypeIndexer permissionTypeIndexer,
            ITypeExplorer typeExplorer)
        {
            this.permissionTypeRegistry = permissionTypeRegistry;
            this.permissionTypeIndexer = permissionTypeIndexer;
            this.typeExplorer = typeExplorer;
        }

        public void OnApplicationStarted()
        {
            permissionTypeIndexer.EnsureIndexed();

            foreach (var commandType in typeExplorer
                                            .GetAllTypes()
                                            .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition)
                                            .Where(x => typeof(ICommandBase).IsAssignableFrom(x)))
            {
                commandTypePermissions[commandType] = GetCommandTypePermissions(commandType).ToList();
            }  
        }

        public IEnumerable<Permission> GetCommandPermissions(ICommandBase command)
        {
            List<Permission> permissions;
            if (commandTypePermissions.TryGetValue(command.GetType(), out permissions))
            {
                return permissions;
            }

            throw new ArgumentException(
                $"Unknown command type to GetCommandPermissions for: {command.GetType().FullName}");
        }

        private IEnumerable<Permission> GetCommandTypePermissions(Type type)
        {
            IEnumerable<Permission> permissions = type
                .GetCustomAttributes(typeof(AuthorizePermissionsAttribute), true)
                .SelectMany(x => ((AuthorizePermissionsAttribute)x).PermissionIds)
                .Select(x => new Permission()
                {
                    PermissionType = permissionTypeRegistry.GetPermissionTypeById(x),
                    ResourceId = null,
                    ContextId = null
                });
            
            return permissions;
        }
    }
}
