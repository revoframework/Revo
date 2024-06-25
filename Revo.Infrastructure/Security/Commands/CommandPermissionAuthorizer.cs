﻿using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Security;

namespace Revo.Infrastructure.Security.Commands
{
    public class CommandPermissionAuthorizer(ICommandPermissionCache commandPermissionCache,
            IPermissionAuthorizationMatcher permissionAuthorizationMatcher,
            IUserContext userContext) : CommandAuthorizer<ICommandBase>
    {
        protected override async Task AuthorizeCommand(ICommandBase command)
        {
            bool isAuthRequired = commandPermissionCache.IsAuthenticationRequired(command);
            if (isAuthRequired && !userContext.IsAuthenticated)
            {
                throw new AuthorizationException(
                    $"User must be authenticated to access command of type '{command.GetType().FullName}'");
            }

            var requiredPermissions = commandPermissionCache.GetCommandPermissions(command);
            var userPermissions = await userContext.GetPermissionsAsync();

            if (!permissionAuthorizationMatcher.CheckAuthorization(userPermissions, requiredPermissions))
            {
                throw new AuthorizationException(
                    $"User is not authorized to access command of type '{command.GetType().FullName}'");
            }
        }
    }
}
