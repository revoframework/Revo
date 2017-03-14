using System.Data.Entity;
using GTRevo.Platform.Core;
using GTRevo.Platform.Transactions;
using Ninject.Modules;

namespace GTRevo.DataAccess.EF6
{
    public class EF6DataAccessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<DbContext>().To<EntityContext>()
                 .InRequestOrJobScope()
                 .WithConstructorArgument("connectionName", "EntityContext");

            Bind<IRepository, ITransactionProvider>().To<Repository>()
                .InRequestOrJobScope();

            Bind<IDatabaseAccess>().To<DatabaseAccess>()
                .InRequestOrJobScope();

            Bind<IModelMetadataExplorer>()
                .To<ModelMetadataExplorer>()
                .InSingletonScope();

            Bind<ModelDefinitionDiscovery>()
                .ToSelf()
                .InSingletonScope();
        }
    }
}
