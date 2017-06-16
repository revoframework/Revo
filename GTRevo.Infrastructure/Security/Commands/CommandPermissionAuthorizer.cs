using System.Threading.Tasks;
using GTRevo.Commands;
using GTRevo.Platform.Security;
using GTRevo.Platform.Security.Identity;

namespace GTRevo.Infrastructure.Security.Commands
{
    public class CommandPermissionAuthorizer : CommandAuthorizer<ICommandBase>
    {
        private readonly CommandPermissionCache commandPermissionCache;
        private readonly PermissionAuthorizer permissionAuthorizer;
        private readonly IUserContext userContext;
        private readonly AppUserManager appUserManager;

        public CommandPermissionAuthorizer(CommandPermissionCache commandPermissionCache,
            PermissionAuthorizer permissionAuthorizer,
            IUserContext userContext,
            AppUserManager appUserManager)
        {
            this.commandPermissionCache = commandPermissionCache;
            this.permissionAuthorizer = permissionAuthorizer;
            this.userContext = userContext;
            this.appUserManager = appUserManager;
        }

        protected override async Task AuthorizeCommand(ICommandBase command)
        {
            var requiredPermissions = commandPermissionCache.GetCommandPermissions(command);
            var user = await userContext.GetUserAsync();
            var userPermissions = await appUserManager.GetUserPermissionsAsync(user);

            if (!permissionAuthorizer.CheckAuthorization(userPermissions, requiredPermissions))
            {
                throw new AuthorizationException(
                    $"User not authorized to access command of type '{command.GetType().FullName}'");
            }
        }
    }
}
