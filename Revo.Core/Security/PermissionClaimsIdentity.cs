using System.Collections.Generic;
using System.Security.Claims;

namespace Revo.Core.Security
{
    public class PermissionClaimsIdentity : ClaimsIdentity
    {
        public PermissionClaimsIdentity(string authenticationType, string nameType, string roleType)
            : base(authenticationType, nameType, roleType)
        {
        }

        /// <summary>
        /// Permissions cached for the lifetime of this identity instance.
        /// </summary>
        public HashSet<Permission> Permissions { get; set; }
    }
}
