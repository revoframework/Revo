using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class EnqueueJobCommandHandler : ICommandHandler<IEnqueueJobCommand>
    {
        private readonly IJobScheduler jobScheduler;

        public EnqueueJobCommandHandler(IJobScheduler jobScheduler)
        {
            this.jobScheduler = jobScheduler;
        }

        public Task HandleAsync(IEnqueueJobCommand command, CancellationToken cancellationToken)
        {
            return jobScheduler.EnqeueJobAsync(GetCommandJob((dynamic)command.Command), command.TimeDelay);
        }

        protected IExecuteCommandJob GetCommandJob<T>(T command) where T : ICommandBase
        {
            return new ExecuteCommandJob<T>(command);
        }
    }
}
