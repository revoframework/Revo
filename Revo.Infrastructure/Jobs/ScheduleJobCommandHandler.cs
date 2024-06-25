using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class ScheduleJobCommandHandler(IJobScheduler jobScheduler) : ICommandHandler<ScheduleJobCommand>
    {
        public Task HandleAsync(ScheduleJobCommand command, CancellationToken cancellationToken)
        {
            return jobScheduler.ScheduleJobAsync(GetCommandJob((dynamic)command.Command), command.EnqueueAt);
        }

        protected IExecuteCommandJob GetCommandJob<T>(T command) where T : ICommandBase
        {
            return new ExecuteCommandJob<T>(command);
        }
    }
}
