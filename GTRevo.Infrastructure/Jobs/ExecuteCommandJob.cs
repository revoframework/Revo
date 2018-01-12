using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Commands;

namespace GTRevo.Infrastructure.Jobs
{
    public class ExecuteCommandJob : IJob
    {
        public ExecuteCommandJob(ICommandBase command)
        {
            Command = command;
        }

        public ICommandBase Command { get; }
        //public Guid ExecutionParameters...
        //tenant context
    }
}
