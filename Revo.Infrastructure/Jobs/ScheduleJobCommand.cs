using System;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class ScheduleJobCommand(ICommandBase command, DateTimeOffset enqueueAt) : ICommand
    {
        public ICommandBase Command { get; } = command;
        public DateTimeOffset EnqueueAt { get; } = enqueueAt;
    }
}
