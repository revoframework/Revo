using System.Data.Entity;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;
using Revo.Core.IO.OData;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.EF6.IO.OData;
using Revo.DataAccess.EF6.Model;
using Revo.DataAccess.Entities;

namespace Revo.DataAccess.EF6
{
    public class EF6DataAccessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<DbContext>().To<EntityContext>()
                 .InTransientScope()
                 .WithConstructorArgument("connectionName", "EntityContext");

            Bind(typeof(ICrudRepository), typeof(IReadRepository), typeof(IEF6CrudRepository),
                    typeof(IEF6ReadRepository))
                .To<EF6CrudRepository>()
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

            Bind<IQueryableToODataResultConverter>()
                .To<EF6QueryableToODataResultConverter>()
                .InSingletonScope();
        }
    }
}
