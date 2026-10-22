using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Revo.Infrastructure.Jobs;

namespace Revo.Hangfire
{
    public class HangfireJobEntryPoint<TJob>
        where TJob : IJob
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        public HangfireJobEntryPoint(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public async Task ExecuteAsync(TJob job)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var jobRunner = scope.ServiceProvider.GetRequiredService<IJobRunner>();
            await jobRunner.RunJobAsync(job, CancellationToken.None);
        }
    }
}
