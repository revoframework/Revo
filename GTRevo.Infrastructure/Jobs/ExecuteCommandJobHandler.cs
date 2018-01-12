using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Commands;

namespace GTRevo.Infrastructure.Jobs
{
    public class ExecuteCommandJobHandler : IJobHandler<ExecuteCommandJob>
    {
        private readonly ICommandBus commandBus;

        public ExecuteCommandJobHandler(ICommandBus commandBus)
        {
            this.commandBus = commandBus;
        }

        public Task HandleAsync(ExecuteCommandJob job, CancellationToken cancellationToken)
        {
            return commandBus.SendAsync(job.Command, cancellationToken);
        }
    }
}
