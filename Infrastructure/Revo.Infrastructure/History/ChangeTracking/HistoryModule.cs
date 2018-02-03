using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;

namespace Revo.Infrastructure.History.ChangeTracking
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
