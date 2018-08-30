using System.Threading;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Jobs
{
    public interface IJobHandler<in T>
        where T : IJob
    {
        Task HandleAsync(T job, CancellationToken cancellationToken);
    }
}
