using System;

namespace Revo.Core.Security
{
    public class Permission
    {
        public Permission(PermissionType permissionType, string resourceId, string contextId)
        {
            PermissionType = permissionType;
            ResourceId = resourceId;
            ContextId = contextId;
        }

        public string ContextId { get; }
        public PermissionType PermissionType { get; }
        public string ResourceId { get; }

        public override bool Equals(object obj)
        {
            Permission other = obj as Permission;
            return other != null
                && other.ResourceId == ResourceId
                && other.ContextId == ContextId
                && Equals(other.PermissionType, PermissionType);
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + (ResourceId?.GetHashCode() ?? -1);
            hash = hash * 23 + (ContextId?.GetHashCode() ?? -1);
            hash = hash * 23 + (PermissionType?.Id.GetHashCode() ?? -1);
            return hash;
        }

        public override string ToString()
        {
            return
                $"Permission {{ PermissionType.Name = {PermissionType?.Name}, ResourceId = {ResourceId}, ContextId = {ContextId} }}";
        }
    }
}
