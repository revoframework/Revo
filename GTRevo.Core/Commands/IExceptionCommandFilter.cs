using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Core.Commands
{
    public interface IExceptionCommandFilter<in T>
        where T : ICommandBase
    {
        Task FilterExceptionAsync(T command, Exception e);
    }
}
