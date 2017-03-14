using System;
using System.Linq;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;
using GTRevo.Platform.Core;
using Ninject;

namespace GTRevo.Platform.Security.WebApi
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

            StandardKernel kernel = (StandardKernel)NinjectWebLoader.Bootstrapper.Kernel;
            PermissionTypeRegistry permissionCache = kernel.Get<PermissionTypeRegistry>();

            if (requiredPermissions == null)
            {
                requiredPermissions = permissionIds.Select(x => new Permission()
                {
                    PermissionType = permissionCache.GetPermissionTypeById(x),
                    ResourceId = null,
                    ContextId = null
                }).ToArray();
            }

            PermissionAuthorizer authorizer = kernel.Get<PermissionAuthorizer>();
            return authorizer.CheckAuthorization(user.Identity, requiredPermissions);
        }
    }
}
