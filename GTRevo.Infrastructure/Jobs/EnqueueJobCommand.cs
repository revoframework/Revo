using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Commands;

namespace GTRevo.Infrastructure.Jobs
{
    public class EnqueueJobCommand : ICommand
    {
        public EnqueueJobCommand(ICommandBase command, TimeSpan? timeDelay = null)
        {
            Command = command;
            TimeDelay = timeDelay;
        }

        public ICommandBase Command { get; }
        public TimeSpan? TimeDelay { get; }
    }
}
