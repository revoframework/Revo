using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Commands;

namespace GTRevo.Infrastructure.Jobs
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
