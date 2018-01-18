using GTRevo.Core.Core;
using GTRevo.Core.Core.Lifecycle;
using Ninject.Modules;

namespace GTRevo.Infrastructure.History.ChangeTracking
{
    public class HistoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IChangeDataTypeCache, IApplicationStartListener>()
                .To<ChangeDataTypeCache>()
                .InSingletonScope();

            Bind<ITrackedChangeRecordConverter>()
                .To<TrackedChangeRecordConverter>()
                .InSingletonScope();

            Bind<IChangeTracker>()
                .To<ChangeTracker>()
                .InRequestOrJobScope();

            Bind<IEntityAttributeChangeLogger>()
                .To<EntityAttributeChangeLogger>()
                .InRequestOrJobScope();
        }
    }
}
