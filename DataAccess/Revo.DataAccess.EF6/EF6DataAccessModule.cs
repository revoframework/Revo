using System.Data.Entity;
using System.Linq;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.IO.OData;
using Revo.Core.Lifecycle;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.EF6.IO.OData;
using Revo.DataAccess.EF6.Model;
using Revo.DataAccess.Entities;
using Revo.Platforms.AspNet.IO.OData;

namespace Revo.DataAccess.EF6
{
    public class EF6DataAccessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<DbContext>().To<EntityContext>()
                 .InTransientScope()
                 .WithConstructorArgument("connectionName", "EntityContext");

            if (!Bindings.Any(x => x.Service == typeof(IEF6CrudRepository)))
            {
                Bind(typeof(ICrudRepository), typeof(IReadRepository), typeof(IEF6CrudRepository),
                        typeof(IEF6ReadRepository))
                    .To<EF6CrudRepository>()
                    .InRequestOrJobScope();
            }

            if (!Bindings.Any(x => x.Service == typeof(ICrudRepositoryFactory<IEF6ReadRepository>)))
            {
                Bind(typeof(ICrudRepositoryFactory<ICrudRepository>),
                        typeof(ICrudRepositoryFactory<IReadRepository>),
                        typeof(ICrudRepositoryFactory<IEF6CrudRepository>),
                        typeof(ICrudRepositoryFactory<IEF6ReadRepository>))
                    .To<EF6CrudRepositoryFactory>()
                    .InRequestOrJobScope();
            }

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
