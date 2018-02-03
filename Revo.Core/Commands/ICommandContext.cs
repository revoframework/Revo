using Revo.Core.Transactions;

namespace Revo.Core.Commands
{
    public interface ICommandContext
    {
        ICommandBase CurrentCommand { get; }
        IUnitOfWork UnitOfWork { get; }
    }
}
