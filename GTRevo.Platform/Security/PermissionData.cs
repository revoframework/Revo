using System;

namespace GTRevo.Platform.Security
{
    public struct PermissionData
    {
        public Guid PermissionTypeId { get; set; }
        public Guid? ResourceId { get; set; }
        public Guid? ContextId { get; set; }
    }
}
