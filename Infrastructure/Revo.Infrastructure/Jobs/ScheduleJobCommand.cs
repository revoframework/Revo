using System;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class ScheduleJobCommand : ICommand
    {
        public ScheduleJobCommand(ICommandBase command, DateTimeOffset enqueueAt)
        {
            Command = command;
            EnqueueAt = enqueueAt;
        }

        public ICommandBase Command { get; }
        public DateTimeOffset EnqueueAt { get; }
    }
}
