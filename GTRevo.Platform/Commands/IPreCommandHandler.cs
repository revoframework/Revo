using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Platform.Commands
{
    public interface IPreCommandHandler<in T>
        where T : ICommandBase
    {
        Task Handle(T command);
    }
}
