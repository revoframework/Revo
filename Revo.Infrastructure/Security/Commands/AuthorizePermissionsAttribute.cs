using System;
using System.Linq;

namespace Revo.Infrastructure.Security.Commands
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
