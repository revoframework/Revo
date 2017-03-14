using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Platform.Commands
{
    public interface ICommandWithContext : ICommand, IHasContext
    {
    }

    public interface ICommandWithContext<out T> : ICommand<T>, IHasContext
    {
    }
}
