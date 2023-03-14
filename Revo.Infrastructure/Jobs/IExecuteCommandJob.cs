using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public interface IExecuteCommandJob : IJob
    {
        ICommandBase Command { get; }
    }
}
