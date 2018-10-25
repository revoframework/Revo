using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Extensions.History.ChangeTracking
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
                .InTaskScope();

            Bind<IEntityAttributeChangeLogger>()
                .To<EntityAttributeChangeLogger>()
                .InTaskScope();
        }
    }
}
