using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Infrastructure.Jobs.Hangfire;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Jobs
{
    public class JobsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IJobScheduler>()
                .To<HangfireJobScheduler>()
                .InRequestOrJobScope();

            Bind<IJobHandler<ExecuteCommandJob>>()
                .To<ExecuteCommandJobHandler>()
                .InRequestOrJobScope();

            Bind<IJobRunner>()
                .To<JobRunner>()
                .InRequestOrJobScope();
        }
    }
}
