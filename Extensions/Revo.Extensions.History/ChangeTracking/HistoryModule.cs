using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.Extensions.History.ChangeTracking
{
    [AutoLoadModule(false)]
    public class HistoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IChangeDataTypeCache, IApplicationStartedListener>()
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

            Bind<ResourceDatabaseMigrationDiscoveryAssembly>()
                .ToConstant(new ResourceDatabaseMigrationDiscoveryAssembly(
                    typeof(HistoryModule).Assembly, "Sql"))
                .InSingletonScope();
        }
    }
}
