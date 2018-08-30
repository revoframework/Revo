using System;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class EnqueueJobCommand<T> : IEnqueueJobCommand
        where T : ICommandBase
    {
        public EnqueueJobCommand(T command, TimeSpan? timeDelay = null)
        {
            Command = command;
            TimeDelay = timeDelay;
        }

        public T Command { get; }
        public TimeSpan? TimeDelay { get; }

        ICommandBase IEnqueueJobCommand.Command => Command;
    }
}
