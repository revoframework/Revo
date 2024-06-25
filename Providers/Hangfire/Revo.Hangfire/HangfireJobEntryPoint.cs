using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Infrastructure.Jobs;

namespace Revo.Hangfire
{
    public class HangfireJobEntryPoint<TJob>(IJobRunner jobRunner)
        where TJob : IJob
    {
        public async Task ExecuteAsync(TJob job)
        {
            using (TaskContext.Enter())
            {
                await jobRunner.RunJobAsync(job, CancellationToken.None); // must await instead of returning, otherwise task context gets disposed too early
            }
        }
    }
}
