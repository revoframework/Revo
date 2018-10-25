using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Infrastructure.Jobs;

namespace Revo.Hangfire
{
    public class HangfireJobEntryPoint<TJob>
        where TJob : IJob
    {
        private readonly IJobRunner jobRunner;

        public HangfireJobEntryPoint(IJobRunner jobRunner)
        {
            this.jobRunner = jobRunner;
        }

        public async Task ExecuteAsync(TJob job)
        {
            using (TaskContext.Enter())
            {
                await jobRunner.RunJobAsync(job); // has to await, otherwise task context gets disposed too early
            }
        }
    }
}
