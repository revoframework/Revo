using Ninject.Modules;
using Revo.Core.Core;

namespace Revo.Infrastructure.Jobs
{
    public class JobsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IJobHandler<IExecuteCommandJob>>()
                .To<ExecuteCommandJobHandler>()
                .InRequestOrJobScope();

            Bind<IJobRunner>()
                .To<JobRunner>()
                .InRequestOrJobScope();
        }
    }
}
