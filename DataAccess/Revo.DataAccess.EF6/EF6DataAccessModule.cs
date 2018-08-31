using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ninject.Extensions.ContextPreservation;
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
    [AutoLoadModule(false)]
    public class EF6DataAccessModule : NinjectModule
    {
        private readonly EF6ConnectionConfiguration connectionConfiguration;
        private readonly bool useAsPrimaryRepository;

        public EF6DataAccessModule(EF6ConnectionConfiguration connectionConfiguration, bool useAsPrimaryRepository)
        {
            this.connectionConfiguration = connectionConfiguration;
            this.useAsPrimaryRepository = useAsPrimaryRepository;
        }

        public override void Load()
        {
            Bind<EF6ConnectionConfiguration>()
                .ToConstant(connectionConfiguration);

            Bind<DbContext>().To<EntityContext>()
                 .InTransientScope()
                 .WithConstructorArgument("connectionName", ctx => ctx.ContextPreservingGet<EF6ConnectionConfiguration>());

            List<Type> repositoryTypes = new List<Type>()
            {
                typeof(IEF6CrudRepository),
                typeof(IEF6ReadRepository)
            };

            if (useAsPrimaryRepository)
            {
                repositoryTypes.AddRange(new[]
                {
                    typeof(ICrudRepository), typeof(IReadRepository)
                });

                Bind<IDatabaseAccess>().To<DatabaseAccess>()
                    .InRequestOrJobScope();
            }

            Bind(repositoryTypes.ToArray())
                .To<EF6CrudRepository>()
                .InRequestOrJobScope();

            Bind(repositoryTypes.Select(x => typeof(ICrudRepositoryFactory<>).MakeGenericType(x)).ToArray())
                .To<EF6CrudRepositoryFactory>()
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

            Bind<IDbContextFactory, IApplicationStartListener>()
                .To<DbContextFactory>()
                .InSingletonScope();

            Bind<IQueryableToODataResultConverter>()
                .To<EF6QueryableToODataResultConverter>()
                .InSingletonScope();
        }
    }
}
