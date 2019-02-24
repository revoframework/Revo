using System.Collections.Generic;
using Revo.Core.Commands;
using Revo.Core.Security;

namespace Revo.Infrastructure.Security.Commands
{
    public interface ICommandPermissionCache
    {
        IReadOnlyCollection<Permission> GetCommandPermissions(ICommandBase command);
        bool IsAuthenticationRequired(ICommandBase command);
    }
}