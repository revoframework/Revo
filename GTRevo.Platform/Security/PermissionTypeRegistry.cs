using System;
using System.Collections.Generic;

namespace GTRevo.Platform.Security
{
    public class PermissionTypeRegistry : IPermissionTypeRegistry
    {
        private Dictionary<string, PermissionType> namesToTypes = new Dictionary<string, PermissionType>();
        private Dictionary<Guid, PermissionType> idsToTypes = new Dictionary<Guid, PermissionType>();
        
        public IEnumerable<PermissionType> PermissionTypes
        {
            get
            {
                return namesToTypes.Values;
            }
        }

        public PermissionType GetPermissionTypeById(Guid permissionId)
        {
            PermissionType permission;
            if (!idsToTypes.TryGetValue(permissionId, out permission))
            {
                throw new ArgumentException("PermissionType with specified ID not found: " + permissionId);
            }

            return permission;
        }

        public PermissionType GetPermissionTypeByName(string permissionName)
        {
            PermissionType permission;
            if (!namesToTypes.TryGetValue(permissionName, out permission))
            {
                throw new ArgumentException("PermissionType with specified name not found: " + permissionName);
            }

            return permission;
        }

        public void RegisterPermissionType(PermissionType permissionType)
        {
            if (namesToTypes.ContainsKey(permissionType.Name))
            {
                throw new ArgumentException("PermissionType with specified name has already been registered: " + permissionType.Name);
            }

            if (idsToTypes.ContainsKey(permissionType.Id))
            {
                throw new ArgumentException("PermissionType with specified ID has already been registered: " + permissionType.Id);
            }

            namesToTypes[permissionType.Name] = permissionType;
            idsToTypes[permissionType.Id] = permissionType;
        }
    }
}
