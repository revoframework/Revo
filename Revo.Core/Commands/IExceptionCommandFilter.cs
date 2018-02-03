using System;
using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    public interface IExceptionCommandFilter<in T>
        where T : ICommandBase
    {
        Task FilterExceptionAsync(T command, Exception e);
    }
}
