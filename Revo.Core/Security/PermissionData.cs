using System;

namespace Revo.Core.Security
{
    public struct PermissionData
    {
        public Guid PermissionTypeId { get; set; }
        public string ResourceId { get; set; }
        public string ContextId { get; set; }
    }
}
