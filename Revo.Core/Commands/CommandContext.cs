using Revo.Core.Transactions;

namespace Revo.Core.Commands
{
    public class CommandContext(ICommandBase currentCommand, IUnitOfWork unitOfWork) : ICommandContext
    {
        public ICommandBase CurrentCommand { get; } = currentCommand;
        public IUnitOfWork UnitOfWork { get; } = unitOfWork;
    }
}
