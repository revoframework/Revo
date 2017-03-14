using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Platform.Security;

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
