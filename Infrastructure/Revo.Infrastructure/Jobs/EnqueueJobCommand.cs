using System;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
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
