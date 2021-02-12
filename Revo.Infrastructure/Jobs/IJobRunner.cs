using System.Threading;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Jobs
{
    public interface IJobRunner
    {
        Task RunJobAsync(IJob job, CancellationToken cancellationToken);
    }
}
