using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;
using Ninject;
using Revo.AspNet.Core;
using Revo.Core.Security;

namespace Revo.AspNet.Security.WebApi
{
    public class AuthorizePermissionsAttribute : AuthorizeAttribute
    {
        private readonly Guid[] permissionIds;
        private Permission[] requiredPermissions;

        public AuthorizePermissionsAttribute(params string[] permissionIds)
        {
            this.permissionIds = permissionIds.Select(x => Guid.Parse(x)).ToArray();
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            IPrincipal user = actionContext.ControllerContext.RequestContext.Principal;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return false; //TODO: allow anonymous permissions
            }

            bool isAuthorized = base.IsAuthorized(actionContext);
            if (!isAuthorized)
            {
                return false;
            }

            if (user.Identity is ClaimsIdentity claimsIdentity)
            {
                IKernel kernel = RevoHttpApplication.Current.Kernel;
                PermissionTypeRegistry permissionCache = kernel.Get<PermissionTypeRegistry>();

                if (requiredPermissions == null)
                {
                    requiredPermissions = permissionIds.Select(x => new Permission(
                        permissionCache.GetPermissionTypeById(x), null, null)).ToArray();
                }

                IPermissionAuthorizationMatcher authorizationMatcher = kernel.Get<IPermissionAuthorizationMatcher>();
                return authorizationMatcher.CheckAuthorization(claimsIdentity, requiredPermissions);
            }
            else
            {
                // only claim-based identities are supported for permission authorization
                return false;
            }
        }
    }
}
