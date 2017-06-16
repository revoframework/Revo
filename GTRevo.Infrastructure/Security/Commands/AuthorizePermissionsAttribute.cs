using System;
using System.Linq;

namespace GTRevo.Infrastructure.Security.Commands
{
    public class AuthorizePermissionsAttribute : Attribute
    {
        public AuthorizePermissionsAttribute(params string[] permissionIds)
        {
            PermissionIds = permissionIds.Select(x => Guid.Parse(x)).ToArray();
        }

        public Guid[] PermissionIds { get; }
    }
}
