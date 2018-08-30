using System;
using System.Collections.Generic;
using System.Text;
using Ninject.Modules;
using Revo.Core.Core;

namespace Revo.Infrastructure.Jobs
{
    public class NullJobsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IJobScheduler>()
                .To<NullJobScheduler>()
                .InSingletonScope();
        }
    }
}
