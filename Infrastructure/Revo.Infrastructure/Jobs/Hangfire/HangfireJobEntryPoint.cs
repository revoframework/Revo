using System.Threading.Tasks;
using Revo.Core.Core;

namespace Revo.Infrastructure.Jobs.Hangfire
{
    public class HangfireJobEntryPoint<TJob>
        where TJob : IJob
    {
        private readonly IJobRunner jobRunner;

        public HangfireJobEntryPoint(IJobRunner jobRunner)
        {
            this.jobRunner = jobRunner;
        }

        public Task ExecuteAsync(TJob job)
        {
            using (TaskContext.Enter())
            {
                return jobRunner.RunJobAsync(job);
            }
        }
    }
}
