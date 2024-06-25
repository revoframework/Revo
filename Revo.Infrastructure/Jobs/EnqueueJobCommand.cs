using System;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class EnqueueJobCommand<T>(T command, TimeSpan? timeDelay = null) : IEnqueueJobCommand
        where T : ICommandBase
    {


        public T Command { get; } = command;
        public TimeSpan? TimeDelay { get; } = timeDelay;

        ICommandBase IEnqueueJobCommand.Command => Command;
    }
}
