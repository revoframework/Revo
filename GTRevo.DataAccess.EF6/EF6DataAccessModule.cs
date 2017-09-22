using System.Data.Entity;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.Core.Transactions;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.DataAccess.EF6.Model;
using GTRevo.DataAccess.Entities;
using GTRevo.Platform.Core;
using Ninject.Modules;

namespace GTRevo.DataAccess.EF6
{
    public class EF6DataAccessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<DbContext>().To<EntityContext>()
                 .InTransientScope()
                 .WithConstructorArgument("connectionName", "EntityContext");

            Bind(typeof(ICrudRepository), typeof(IReadRepository), typeof(IEF6CrudRepository),
                    typeof(IEF6ReadRepository), typeof(ITransactionProvider))
                .To<EF6CrudRepository>()
                .InRequestOrJobScope();

            Bind<IEF6RawQueryRepository>()
                .To<EF6RawQueryRepository>()
                .InRequestOrJobScope();

            Bind<IDatabaseAccess>().To<DatabaseAccess>()
                .InRequestOrJobScope();

            Bind<IModelMetadataExplorer, IApplicationStartListener>()
                .To<ModelMetadataExplorer>()
                .InSingletonScope();

            Bind<ModelDefinitionDiscovery>()
                .ToSelf()
                .InSingletonScope();

            Bind<EntityTypeDiscovery>()
                .ToSelf()
                .InSingletonScope();

            /*Bind<IConvention>()
                .To<CustomStoreConvention>()
                .InTransientScope();*/

            Bind<IDbContextFactory, IApplicationStartListener>()
                .To<DbContextFactory>()
                .InSingletonScope();
        }
    }
}
