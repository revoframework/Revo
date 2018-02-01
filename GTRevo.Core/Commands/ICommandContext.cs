using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;
using Microsoft.Runtime.CompilerServices;

namespace GTRevo.Core.Commands
{
    public interface ICommandContext
    {
        ICommandBase CurrentCommand { get; }
        IUnitOfWork UnitOfWork { get; }
    }
}
