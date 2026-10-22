using Revo.Core.Commands;
using Revo.Infrastructure.Security.Commands;

namespace Revo.IntegrationTests.TestApp.Messages.Commands
{
    [Authenticated]
    public class AuthenticatedOnlyCommand : ICommand
    {
    }
}
