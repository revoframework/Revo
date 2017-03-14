using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Platform.Commands
{
    public interface ICommandRequest
    {
        ICommand Command { get; }
        IDictionary<Type, object> Data { get; }
    }
}
