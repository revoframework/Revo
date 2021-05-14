using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.Jobs.InMemory
{
    [AutoLoadModule(false)]
    public class InMemoryJobsModule : NinjectModule
    {
        private readonly IInMemoryJobSchedulerConfiguration schedulerConfiguration;
        private readonly bool isActive;

        public InMemoryJobsModule(IInMemoryJobSchedulerConfiguration schedulerConfiguration, bool isActive)
        {
            this.schedulerConfiguration = schedulerConfiguration;
            this.isActive = isActive;
        }

        public override void Load()
        {
            Bind<IInMemoryJobWorkerProcess, IApplicationStartedListener, IApplicationStoppingListener>()
                .To<InMemoryJobWorkerProcess>()
                .InSingletonScope();

            Bind<IInMemoryJobSchedulerProcess, IApplicationStartedListener, IApplicationStoppingListener>()
                .To<InMemoryJobSchedulerProcess>()
                .InSingletonScope();

            Bind<IInMemoryJobSchedulerConfiguration>()
                .ToConstant(schedulerConfiguration)
                .InSingletonScope();

            if (isActive)
            {
                Bind<IJobScheduler, IInMemoryJobScheduler>()
                    .To<InMemoryJobScheduler>()
                    .InSingletonScope();
            }
            else
            {
                Bind<IInMemoryJobScheduler>()
                    .To<InMemoryJobScheduler>()
                    .InSingletonScope();
            }
        }
    }
}
