using System;
using System.Collections.Generic;
using Revo.Core.ValueObjects;

namespace Revo.Core.Security
{
    public class Permission : ValueObject<Permission>
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

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            yield return (nameof(ContextId), ContextId);
            yield return (nameof(PermissionType), PermissionType);
            yield return (nameof(ResourceId), ResourceId);
        }
    }
}
