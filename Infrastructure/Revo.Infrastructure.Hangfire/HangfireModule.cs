using System;
using System.Collections.Generic;
using System.Text;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Infrastructure.Jobs;

namespace Revo.Infrastructure.Hangfire
{
    [AutoLoadModule(false)]
    public class HangfireModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IJobScheduler>()
                .To<HangfireJobScheduler>()
                .InRequestOrJobScope();
        }
    }
}
