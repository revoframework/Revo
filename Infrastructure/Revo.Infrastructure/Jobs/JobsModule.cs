using Ninject.Modules;
using Revo.Core.Core;
using Revo.Infrastructure.Jobs.Hangfire;

namespace Revo.Infrastructure.Jobs
{
    public class JobsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IJobScheduler>()
                .To<HangfireJobScheduler>()
                .InRequestOrJobScope();

            Bind<IJobHandler<IExecuteCommandJob>>()
                .To<ExecuteCommandJobHandler>()
                .InRequestOrJobScope();

            Bind<IJobRunner>()
                .To<JobRunner>()
                .InRequestOrJobScope();
        }
    }
}
