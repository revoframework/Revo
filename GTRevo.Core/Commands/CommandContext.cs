using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;

namespace GTRevo.Core.Commands
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
