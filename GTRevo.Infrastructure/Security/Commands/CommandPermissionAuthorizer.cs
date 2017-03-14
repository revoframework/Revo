using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Platform.Commands;

namespace GTRevo.Infrastructure.Security.Commands
{
    public class CommandPermissionAuthorizer : CommandAuthorizer<ICommandBase>
    {
        protected override async Task AuthorizeCommand(ICommandBase command)
        {

        }
    }
}
