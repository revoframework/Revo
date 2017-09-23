using System;

namespace GTRevo.Core.Security
{
    public class Permission
    {
        public Guid? ResourceId { get; set; }
        public Guid? ContextId { get; set; }
        public PermissionType PermissionType { get; set; }

        public override bool Equals(object obj)
        {
            Permission other = obj as Permission;
            return other != null
                && other.ResourceId == ResourceId
                && other.ContextId == ContextId
                && other.PermissionType?.Id == PermissionType?.Id;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + (ResourceId?.GetHashCode() ?? -1);
            hash = hash * 23 + (ContextId?.GetHashCode() ?? -1);
            hash = hash * 23 + (PermissionType?.Id.GetHashCode() ?? -1);
            return hash;
        }
    }
}
