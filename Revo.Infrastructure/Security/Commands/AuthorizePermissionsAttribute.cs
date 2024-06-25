using System;
using System.Linq;

namespace Revo.Infrastructure.Security.Commands
{
    public class AuthorizePermissionsAttribute(params string[] permissionIds) : Attribute
    {
        public Guid[] PermissionIds { get; } = permissionIds.Select(x => Guid.Parse(x)).ToArray();
    }
}
