using System.Threading.Tasks;

namespace Revo.Infrastructure.Jobs
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
            return jobRunner.RunJobAsync(job);
        }
    }
}
