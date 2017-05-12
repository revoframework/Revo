using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;
using GTRevo.Platform.Transactions;
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

            Bind<ICrudRepository, IReadRepository, ITransactionProvider>().To<CrudRepository>()
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
