using Revo.Core.Transactions;

namespace Revo.Core.Commands
{
    public class CommandContext : ICommandContext
    {
        public CommandContext(ICommandBase currentCommand, IUnitOfWork unitOfWork)
        {
            CurrentCommand = currentCommand;
            UnitOfWork = unitOfWork;
        }

        public ICommandBase CurrentCommand { get; }
        public IUnitOfWork UnitOfWork { get; }
    }
}
