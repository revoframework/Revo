using Ninject.Modules;
using Revo.Core.Core;
using Revo.Infrastructure.Jobs;

namespace Revo.Hangfire
{
    [AutoLoadModule(false)]
    public class HangfireModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IJobScheduler>()
                .To<HangfireJobScheduler>()
                .InTaskScope();
        }
    }
}
