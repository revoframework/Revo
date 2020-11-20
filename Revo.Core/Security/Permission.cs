using System;
using System.Collections.Generic;
using Revo.Core.ValueObjects;

namespace Revo.Core.Security
{
    public class Permission : ValueObject<Permission>
    {
        public Permission(PermissionType permissionType, object resourceId, object contextId)
        {
            PermissionTypeId = permissionType.Id;
            ResourceId = resourceId;
            ContextId = contextId;
        }

        public Permission(Guid permissionTypeId, object resourceId, object contextId)
        {
            PermissionTypeId = permissionTypeId;
            ResourceId = resourceId;
            ContextId = contextId;
        }

        public Guid PermissionTypeId { get; }
        public object ContextId { get; }
        public object ResourceId { get; }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(ContextId), ContextId);
            yield return (nameof(PermissionTypeId), PermissionTypeId);
            yield return (nameof(ResourceId), ResourceId);
        }
    }
}
