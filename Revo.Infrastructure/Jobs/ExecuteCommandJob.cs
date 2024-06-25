using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class ExecuteCommandJob<T>(T command) : IExecuteCommandJob
        where T : ICommandBase
    {


        public T Command { get; } = command;

        //public Guid ExecutionParameters...
        //tenant context

        ICommandBase IExecuteCommandJob.Command => Command;
    }
}
