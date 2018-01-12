using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Commands;

namespace GTRevo.Core.Commands
{
    public interface IPreCommandFilter<in T>
        where T : ICommandBase
    {
        Task PreFilterAsync(T command);
    }
}
