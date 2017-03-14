using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Platform.Commands
{
    public class CommandRequest : ICommandRequest
    {
        public CommandRequest(ICommand command)
        {
            Command = command;
        }

        public ICommand Command { get; private set; }
        public IDictionary<Type, object> Data { get; private set; } = new Dictionary<Type, object>();
    }
}
