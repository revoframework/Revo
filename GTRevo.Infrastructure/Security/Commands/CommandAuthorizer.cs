using System.Threading.Tasks;
using GTRevo.Core.Commands;

namespace GTRevo.Infrastructure.Security.Commands
{
    public abstract class CommandAuthorizer<T> : ICommandFilter<T>
        where T : ICommandBase
    {
        public Task Handle(T command)
        {
            return AuthorizeCommand(command);
        }

        protected abstract Task AuthorizeCommand(T command);
    }
}
